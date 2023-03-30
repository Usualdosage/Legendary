// <copyright file="GlobalSuppressions.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore", Justification = "Easier to read this way.", Scope = "namespace", Target = "~F:Legendary.Core.Types")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly", Justification = "This is a fault in the analyzer.", Scope = "member", Target = "~M:Legendary.Core.Contracts.ILanguageProcessor.Process(Legendary.Core.Models.Character,Legendary.Core.Models.Mobile,System.String)~System.Threading.Tasks.Task{System.String[]}")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly", Justification = "This is a fault in the analyzer.", Scope = "member", Target = "~M:Legendary.Core.Contracts.ILanguageProcessor.Process(Legendary.Core.Models.Character,Legendary.Core.Models.Mobile,System.String,System.Threading.CancellationToken)~System.Threading.Tasks.Task{System.String[]}")]
