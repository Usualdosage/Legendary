// <copyright file="CompanionProcessor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Web.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Output;
    using Legendary.Networking.Models;
    using Legendary.Web.Contracts;
    using Legendary.Web.Models;
    using MongoDB.Driver;
    using Newtonsoft.Json;
    using Persona = Legendary.Web.Models.Persona;

    /// <summary>
    /// Processor class for companions (testing personas).
    /// </summary>
    public class CompanionProcessor : ICompanionProcessor
    {
        private readonly string? apiKey;
        private readonly string url = "https://api.openai.com/v1/chat/completions";
        private Dictionary<string, List<dynamic>> personaTrainingData;
        private IMongoCollection<Persona> personas;
        private IMongoCollection<PersonaMemory> memories;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompanionProcessor"/> class.
        /// </summary>
        /// <param name="serverSettings">The server settings from Azure App config.</param>
        public CompanionProcessor(IServerSettings serverSettings)
        {
            this.apiKey = serverSettings.ChatGPTAPIKey;
            MongoClient dbClient = new (serverSettings.MongoConnectionString);
            var database = dbClient.GetDatabase("Legacy");
            this.personas = database.GetCollection<Persona>("Companions");
            this.memories = database.GetCollection<PersonaMemory>("CompanionMemories");
            this.personaTrainingData = new Dictionary<string, List<dynamic>>();
        }

        /// <summary>
        /// Executes a request against ChatGPT to process an input.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="persona">The persona.</param>
        /// <param name="message">The input message.</param>
        /// <returns>Output message.</returns>
        public async Task<string> ProcessChat(string userName, string persona, string message)
        {
            try
            {
                if (!this.personaTrainingData.ContainsKey(persona))
                {
                    this.personaTrainingData.Add(persona, await this.Train(persona, userName));
                }

                var response = await this.Chat(userName, this.personaTrainingData[persona], message);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    await this.AddMemory(userName, persona, message);
                    return response;
                }
                else
                {
                    return "Sorry, I'm not available to chat right now.";
                }
            }
            catch
            {
                return "Sorry, I'm not available to chat right now.";
            }
        }

        private async Task<string> Chat(string userName, List<dynamic> trainingData, string message)
        {
            trainingData.Add(new { role = "user", content = $"{userName}: {message}" });

            // Create the request for the API sending the latest collection of chat messages
            var request = new
            {
                messages = trainingData,
                model = "gpt-4",
                max_tokens = 2048,
            };

            try
            {
                // Send the request and capture the response
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.apiKey}");

                var requestJson = JsonConvert.SerializeObject(request);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                using var httpResponseMessage = await httpClient.PostAsync(this.url, requestContent);

                var jsonString = await httpResponseMessage.Content.ReadAsStringAsync();

                var responseObject = JsonConvert.DeserializeAnonymousType(jsonString, new
                {
                    choices = new[] { new { message = new { role = string.Empty, content = string.Empty } } },
                    error = new { message = string.Empty },
                });

                if (responseObject != null)
                {
                    // Check for errors
                    if (!string.IsNullOrEmpty(responseObject?.error?.message))
                    {
                        return responseObject.error.message;
                    }
                    else
                    {
                        // Add the message object to the message collection so the bot "remembers" for the session.
                        var messageObject = responseObject?.choices[0]?.message;

                        if (messageObject != null)
                        {
                            trainingData.Add(messageObject);
                            return messageObject.content;
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<List<dynamic>> Train(string persona, string userName)
        {
            var trainingInformation = new List<string>();

            // Load the base persona file.
            var basePersona = await this.personas.Find(p => p.Id == 1).FirstOrDefaultAsync();

            // Apply the training data.
            if (basePersona != null)
            {
                trainingInformation.AddRange(basePersona.Training);
            }

            // Load the persona
            var personaModel = await this.personas.Find(p => p.Name.ToLower() == persona.ToLower()).FirstOrDefaultAsync();

            if (personaModel != null)
            {
                trainingInformation.AddRange(personaModel.Training);

                var memories = await this.memories.Find(m => m.Username == userName && m.PersonaId == personaModel.Id).FirstOrDefaultAsync();

                if (memories != null)
                {
                    trainingInformation.Add($"You last spoke to {userName} on {memories.LastInteraction} (UTC time).");
                    trainingInformation.Add($"You have the following memories of {userName}:");
                    trainingInformation.AddRange(memories.Memories);
                }
            }

            var messages = new List<dynamic>
            {
                new
                {
                    role = "system",
                    content = string.Join(" ", trainingInformation),
                },
                new
                {
                    role = "assistant",
                    content = "Hello, how are you?",
                },
            };

            return messages;
        }

        private async Task AddMemory(string userName, string persona, string memory)
        {
            var personaObject = await this.personas.Find(p => p.Name == persona).FirstOrDefaultAsync();

            if (personaObject != null)
            {
                var memoryObject = await this.memories.Find(m => m.Username == userName && m.PersonaId == personaObject.Id).FirstOrDefaultAsync();

                if (memoryObject == null)
                {
                    memoryObject = new PersonaMemory()
                    {
                        PersonaId = personaObject.Id,
                        Username = userName,
                        LastInteraction = DateTime.UtcNow,
                        Memories = new List<string>() { memory },
                    };

                    await this.memories.InsertOneAsync(memoryObject);
                }
                else
                {
                    var newMemory = new PersonaMemory()
                    {
                        PersonaId = personaObject.Id,
                        Username = userName,
                        LastInteraction = DateTime.UtcNow,
                    };

                    newMemory.Memories.AddRange(memoryObject.Memories);
                    newMemory.Memories.Add(memory);

                    await this.memories.ReplaceOneAsync(m => m.Id == memoryObject.Id, newMemory);
                }
            }
        }
    }
}
