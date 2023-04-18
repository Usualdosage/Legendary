// <copyright file="ChatGPTService.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.AreaBuilder.Services
{
    using System.Text;
    using Legendary.Core.Models;
    using Microsoft.CognitiveServices.Speech;
    using Newtonsoft.Json;
    using Spectre.Console;

    /// <summary>
    /// Service to call and interact with ChatGPT.
    /// </summary>
    public class ChatGPTService
    {
        private readonly string apiKey;
        private readonly string url = "https://api.openai.com/v1/chat/completions";
        private readonly string imageUrl = "https://api.openai.com/v1/images/generations";

        private readonly string[] maleVoices = new string[7]
        {
            "en-GB-AlfieNeural",
            "en-GB-EthanNeural",
            "en-GB-ElliottNeural",
            "en-GB-NoahNeural",
            "en-GB-OliverNeural",
            "en-GB-RyanNeural",
            "en-GB-ThomasNeural",
        };

        private readonly string[] femaleVoices = new string[7]
        {
            "en-GB-AbbiNeural",
            "en-GB-BellaNeural",
            "en-GB-HollieNeural",
            "en-GB-LibbyNeural",
            "en-GB-MaisieNeural",
            "en-GB-OliviaNeural",
            "en-GB-SoniaNeural",
        };

        private List<dynamic>? trainingData = null;
        private bool modelTrained = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatGPTService"/> class.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        public ChatGPTService(string apiKey = "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")
        {
            this.apiKey = apiKey;
        }

        /// <summary>
        /// Converts the given text to speech.
        /// </summary>
        /// <param name="text">The text to say.</param>
        /// <param name="voice">The voice to use.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public static async Task TextToSpeech(string text, string voice)
        {
            // For more samples please visit https://github.com/Azure-Samples/cognitive-services-speech-sdk

            // Creates an instance of a speech config with specified subscription key and service region.
            string subscriptionKey = "b41156137937421cb313dad217cc35f5";
            string subscriptionRegion = "eastus";

            var config = SpeechConfig.FromSubscription(subscriptionKey, subscriptionRegion);

            // Note: the voice setting will not overwrite the voice element in input SSML.
            config.SpeechSynthesisVoiceName = voice;

            // use the default speaker as audio output.
            using var synthesizer = new SpeechSynthesizer(config);
            using var result = await synthesizer.SpeakTextAsync(text);
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                // Console.WriteLine($"Speech synthesized for text [{text}]");

                // using (MemoryStream ms = new MemoryStream(result.AudioData))
                // {
                //    // Construct the sound player
                //    SoundPlayer player = new SoundPlayer(ms);
                //    player.Play();
                // }
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                    Console.WriteLine($"CANCELED: Did you update the subscription info?");
                }
            }
        }

        /// <summary>
        /// Used to describe a thing.
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <returns>String.</returns>
        public string Describe(string prompt)
        {
            var trainingInformation = new List<string>
            {
                $"Speak in a second person narrative voice.",
                $"As vividly as possible, describe from the perspective of an omniscient narrator in the medieval fantasy world of Mystra.",
                $"Your description should be between four and ten sentences.",
                $"Speak as if you are narrating a fantasy story from a player's perspective.",
                $"You can use sights, sounds, smells to further describe what the player is seeing.",
            };

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
                    content = prompt.ToLower(),
                },
            };

            // Create the request for the API sending the latest collection of chat messages
            var request = new
            {
                messages,
                model = "gpt-4",
                max_tokens = 1024,
            };

            try
            {
                // Send the request and capture the response
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.apiKey}");

                var requestJson = JsonConvert.SerializeObject(request);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                using var httpResponseMessage = httpClient.PostAsync(this.url, requestContent).Result;

                var jsonString = httpResponseMessage.Content.ReadAsStringAsync().Result;

                var responseObject = JsonConvert.DeserializeAnonymousType(jsonString, new
                {
                    choices = new[] { new { message = new { role = string.Empty, content = string.Empty } } },
                    error = new { message = string.Empty },
                });

                if (responseObject != null)
                {
                    if (!string.IsNullOrEmpty(responseObject?.error?.message))
                    {
                        // Check for errors
                        return Markup.Escape(responseObject.error.message);
                    }
                    else
                    {
                        // Add the message object to the message collection so the bot "remembers"
                        var messageObject = responseObject?.choices[0]?.message;

                        if (messageObject != null)
                        {
                            // HOLY CRAP THIS WORKS
                            // this.TextToSpeech(messageObject.content);
                            return Markup.Escape(messageObject.content);
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
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Hits the chat endpoint for the ChatGPT API.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<string> Chat(string message)
        {
            if (!this.modelTrained || this.trainingData == null)
            {
                throw new Exception("Please call Train() on this instance before initializing chat.");
            }

            this.trainingData.Add(new { role = "user", content = message });

            // Create the request for the API sending the latest collection of chat messages
            var request = new
            {
                messages = this.trainingData,
                model = "gpt-4",
                max_tokens = 1024,
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
                    if (!string.IsNullOrEmpty(responseObject?.error?.message))
                    {
                        // Check for errors
                        return Markup.Escape(responseObject.error.message);
                    }
                    else
                    {
                        // Add the message object to the message collection so the bot "remembers"
                        var messageObject = responseObject?.choices[0]?.message;

                        if (messageObject != null)
                        {
                            this.trainingData.Add(messageObject);
                            return Markup.Escape(messageObject.content);
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
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Generates an AI image using DALL-E from a prompt.
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <returns>URL.</returns>
        public string? Image(string prompt)
        {
            try
            {
                var request = new
                {
                    prompt = FormatPrompt(prompt),
                    n = 1,
                    size = "512x512",
                };

                // Send the request and capture the response
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.apiKey}");

                var requestJson = JsonConvert.SerializeObject(request);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                using var httpResponseMessage = httpClient.PostAsync(this.imageUrl, requestContent).Result;

                var jsonString = httpResponseMessage.Content.ReadAsStringAsync().Result;

                var responseObject = JsonConvert.DeserializeAnonymousType(jsonString, new
                {
                    data = new[] { new { url = string.Empty } },
                });

                if (responseObject == null || responseObject.data == null)
                {
                    return null;
                }

                return responseObject?.data[0]?.url;
            }
            catch (Exception exc)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets a random TTS voice.
        /// </summary>
        /// <param name="gender">The gender.</param>
        /// <returns>String.</returns>
        public string GetRandomVoice(SynthesisVoiceGender gender)
        {
            Random rand = new ();

            return gender switch
            {
                SynthesisVoiceGender.Female => this.femaleVoices[rand.Next(0, 6)],
                _ => this.maleVoices[rand.Next(0, 6)],
            };
        }

        /// <summary>
        /// Returns 8 images from a prompt using AI.
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <returns>List of URLs.</returns>
        public List<string>? Images(string prompt)
        {
            var request = new
            {
                prompt = FormatPrompt(prompt),
                n = 8,
                size = "512x512",
            };

            // Send the request and capture the response
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.apiKey}");

            var requestJson = JsonConvert.SerializeObject(request);
            var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

            using var httpResponseMessage = httpClient.PostAsync(this.imageUrl, requestContent).Result;

            var jsonString = httpResponseMessage.Content.ReadAsStringAsync().Result;

            var responseObject = JsonConvert.DeserializeAnonymousType(jsonString, new
            {
                data = new[] { new { url = string.Empty } },
            });

            return responseObject?.data?.Select(d => d.url).ToList();
        }

        /// <summary>
        /// Trains the AI on a given persona.
        /// </summary>
        /// <param name="persona">The persona.</param>
        public void Train(Persona persona)
        {
            var trainingInformation = new List<string>
            {
                $"Your name is {persona.Name}.",
                $"Your race is {persona.Race}.",
                $"Your age is {persona.Age}.",
                $"Your class is {persona.Class}.",
                $"Your attitude is {persona.Attitude}.",
            };

            if (persona.Background != null)
            {
                trainingInformation.AddRange(persona.Background);
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
                    content = persona.Prompt,
                },
            };

            this.trainingData = messages;

            this.modelTrained = true;
        }

        private static string FormatPrompt(string prompt)
        {
            if (prompt.Length < 400)
            {
                return prompt;
            }
            else
            {
                var promptParts = prompt.Split('.');

                StringBuilder sbFormatted = new StringBuilder();

                foreach (var part in promptParts)
                {
                    var max = sbFormatted.Length + part.Length;
                    if (max < 400)
                    {
                        sbFormatted.Append($"{part} ");
                    }
                    else
                    {
                        return sbFormatted.ToString();
                    }
                }

                return sbFormatted.ToString();
            }
        }
    }
}
