using Patrick.Models;
using Patrick.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class HangmanCommand : BaseCommand
    {
        private const char KnownPlaceholder = '_';

        private readonly IHttpService httpService;

        // key: channel of Discord server
        // value: Hangman structure
        private readonly IDictionary<ulong, Hangman> games = new Dictionary<ulong, Hangman>();

        public HangmanCommand(IHttpService httpService) : base("hangman")
        {
            this.httpService = httpService;
            UseEmbed = true;
            Description = "[Hangman game.](https://en.wikipedia.org/wiki/Hangman_(game))";
            Usage = @$"
To start playing, type `!{Name} begin`
!{Name} <text>, where <text> may be either *begin* to start the game, *hint* to request for hints, *surrender* to give up, or a letter to guess the current hangman word.
";
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {

            if (!games.TryGetValue(user.SessionId, out var game))
            {
                game = new Hangman(httpService, user.Fullname);
                games[user.SessionId] = game;
            }

            if (game.IsCompleted)
            {
                if (games.Remove(user.SessionId))
                {
                    game = new Hangman(httpService, user.Fullname);
                    games[user.SessionId] = game;
                }
            }

            if (string.IsNullOrEmpty(user.MessageArgument) || game.Status == HangmanGameStatus.Idle)
            {
                if (game.Status == HangmanGameStatus.Idle)
                    return new CommandResponse(Name, "Game not started yet.");
                else
                    return new CommandResponse(Name, "No letter or word found.");
            }

            var action = GetAction(user.MessageArgument);

            switch (action)
            {
                case HangmanUserAction.Begin:
                    {
                        var success = await game.Begin();
                        if (success)
                            return new CommandResponse(Name,
                                GenerateDefaultResponse(game, action, $"Hangman initiated by: __{game.Initiator}__"));
                        else
                            return new CommandResponse(Name, "There was an error initiating the game :(");
                    }

                case HangmanUserAction.Guess:
                    {
                        var letter = user.MessageArgument!.First();
                        var status = await game.Guess(letter);

                        if (status != HangmanResponseStatus.Error)
                            game.UpdateParticipants(user, status);
                        else
                            return new CommandResponse(Name,
                                GenerateDefaultResponse(game, action, "Oopsie! Something went wrong."));

                        if (status == HangmanResponseStatus.Correct && game.IsCompleted)
                            return new CommandResponse(Name, GameCompletedResponse(game, user));

                        var statusText = status switch
                        {
                            HangmanResponseStatus.AlreadyGuessed => "Already guessed.",
                            HangmanResponseStatus.Correct => ":white_check_mark: Correct!",
                            HangmanResponseStatus.Wrong => ":warning: Wrong!",
                            _ => "Undefined."
                        };
                        return new CommandResponse(Name,
                            GenerateDefaultResponse(game, action, statusText));
                    }

                case HangmanUserAction.Undefined:
                    {
                        if (game.Status != HangmanGameStatus.Started)
                            return new CommandResponse(Name,
                                GenerateDefaultResponse(game, action, "You're playing wrong homie!"));
                        else
                            return new CommandResponse(Name,
                                GenerateDefaultResponse(game, action, "Hmm..."));
                    }

                case HangmanUserAction.Hint:
                    {
                        var hint = await game.Hint();
                        return new CommandResponse(Name,
                            GenerateDefaultResponse(game, action, $"Hint: {hint}"));
                    }

                case HangmanUserAction.Solution:
                    {
                        var status = await game.Solution(user.MessageArgument);

                        if (status != HangmanResponseStatus.Error)
                            game.UpdateParticipants(user, status);
                        else
                            return new CommandResponse(Name,
                                GenerateDefaultResponse(game, action, "Oopsie! Something went wrong."));

                        if (status == HangmanResponseStatus.Correct && game.IsCompleted)
                            return new CommandResponse(Name, GameCompletedResponse(game, user));
                        else
                            return new CommandResponse(Name, 
                                GenerateDefaultResponse(game, action, "Wrong guess homie!"));

                    }

                case HangmanUserAction.Surrender:
                    {
                        var solution = await game.Surrender(user.MessageArgument);

                        if (string.IsNullOrEmpty(solution))
                            return new CommandResponse(Name,
                                GenerateDefaultResponse(game, action, "Something went wrong."));

                        return new CommandResponse(Name, @$"
You surrendered :(

Correct word is: **{solution}**

Try again next time amigo!
");
                    }
                default: return new CommandResponse(Name, "In case this one gets executed, there's really some fucked up happening.");
            }
        }

        private static string GameCompletedResponse(Hangman game, IUser user)
        {
            var scoreList = game.Participants.Select(e => new
            {
                Name = e.Key.Fullname,
                e.Value.GuessCount,
                e.Value.CorrectGuesses,
                Score = e.Value.CorrectGuesses / (double)e.Value.GuessCount * 100
            });
            var scoreboardText = string.Join('\n',
                scoreList
                    .OrderByDescending(e => e.Score)
                    .Select((e, i) => $"{i + 1}.\t__{e.Name}__: **{e.Score:N0}** ({e.GuessCount}/{e.CorrectGuesses})")
            );

            return @$"
:tada::fireworks: Winner winner! Chicken dinner! :fireworks::tada:

Last guessed was by __{user.Fullname}__

Correct word is: **{game.LastStatusWord}**

Participant's score:
{scoreboardText}
";
        }

        private static string GenerateDefaultResponse(Hangman game, HangmanUserAction action, string extra)
        {
            return @$"
{extra}

**action**: {action}
**word**: {game.SanitizedStatusWord} `({game.CountOfLettersToGuess}/{game.RemainingLettersToGuess})`
**progress**: {game.Progress * 100:N0}%
";
        }

        private static HangmanUserAction GetAction(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return HangmanUserAction.Undefined;

            if (string.Equals(text, "hint", StringComparison.OrdinalIgnoreCase))
                return HangmanUserAction.Hint;

            if (string.Equals(text, "begin", StringComparison.OrdinalIgnoreCase))
                return HangmanUserAction.Begin;

            if (string.Equals(text, "surrender", StringComparison.OrdinalIgnoreCase))
                return HangmanUserAction.Surrender;

            if (text.Length > 1)
                return HangmanUserAction.Solution;

            return HangmanUserAction.Guess;
        }

        enum HangmanUserAction
        {
            Undefined,
            Begin,
            Guess,
            Hint,
            Surrender,
            Solution
            // No Quitting!
        }

        enum HangmanGameStatus
        {
            Idle,
            Started
        }

        enum HangmanResponseStatus
        {
            Wrong,
            Correct,
            AlreadyGuessed,
            Error
        }

        class HangmanDto
        {
            [JsonPropertyName("token")]
            public string? Token { get; set; }
            [JsonPropertyName("correct")]
            public bool Correct { get; set; }
            [JsonPropertyName("letter")]
            public string? Letter { get; set; }
            [JsonPropertyName("hangman")]
            public string? Hangman { get; set; }
            [JsonPropertyName("hint")]
            public string? Hint { get; set; }
            [JsonPropertyName("solution")]
            public string? Solution { get; set; }
        }

        class Score
        {
            public int GuessCount { get; set; }
            public int CorrectGuesses { get; set; }
        }

        class Hangman
        {
            private static readonly Uri ApiAddress = new Uri("https://hangman-api.herokuapp.com/hangman");

            private readonly Dictionary<IUser, Score> participants = new Dictionary<IUser, Score>();

            private readonly IHttpService httpService;

            public Hangman(IHttpService httpService, string? initiator)
            {
                this.httpService = httpService;
                Initiator = initiator;
            }

            public void UpdateParticipants(IUser user, HangmanResponseStatus status)
            {
                if (!participants.TryGetValue(user, out var score))
                    score = new Score();
                score.CorrectGuesses += status == HangmanResponseStatus.Correct ? 1 : 0;
                score.GuessCount++;
                participants[user] = score;
            }

            public async Task<bool> Begin(CancellationToken cancellationToken = default)
            {
                try
                {
                    var response = await httpService.PostJson<HangmanDto>(ApiAddress, 
                        cancellationToken);

                    if (response == null)
                        return false;

                    Token = response.Token;

                    if (string.IsNullOrEmpty(response.Token))
                        return false;

                    Status = HangmanGameStatus.Started;
                    LastStatusWord = response.Hangman;
                    CountOfLettersToGuess = response.Hangman!.Length;
                    RemainingLettersToGuess = 0;

                    return true;
                }
                catch (ArgumentNullException) { return false; }
                catch (HttpRequestException) { return false; }
                catch (JsonException) { return false; }
                catch (NotSupportedException) { return false; }
            }

            public async Task<string?> Hint(CancellationToken cancellationToken = default)
            {
                try
                {
                    var address = $"{ApiAddress.AbsoluteUri}/hint?token={Token}";
                    var response = await httpService.Get<HangmanDto>(new Uri(address),
                        cancellationToken: cancellationToken);

                    if (response == null)
                        return null;

                    if (string.IsNullOrEmpty(response.Token))
                        return null;

                    return response.Hint;
                }
                catch (ArgumentNullException) { return null; }
                catch (HttpRequestException) { return null; }
                catch (JsonException) { return null; }
                catch (NotSupportedException) { return null; }
            }

            public async Task<HangmanResponseStatus> Solution(string? text, CancellationToken cancellationToken = default)
            {
                var address = $"{ApiAddress.AbsoluteUri}?token={Token}";
                var response = await httpService.Get<HangmanDto>(new Uri(address),
                    cancellationToken);
                if (response == null)
                    return HangmanResponseStatus.Error;

                var result = string.Equals(response.Solution, text?.Trim(), StringComparison.OrdinalIgnoreCase);

                if (result)
                {
                    RemainingLettersToGuess = CountOfLettersToGuess;
                    LastStatusWord = text;
                }

                return result ? HangmanResponseStatus.Correct : HangmanResponseStatus.Wrong;
            }

            public async Task<string?> Surrender(string? text, CancellationToken cancellationToken = default)
            {
                var address = $"{ApiAddress.AbsoluteUri}?token={Token}";
                var response = await httpService.Get<HangmanDto>(new Uri(address),
                    cancellationToken);
                if (response == null)
                    return null;

                if (string.IsNullOrEmpty(response.Solution))
                    return null;

                RemainingLettersToGuess = CountOfLettersToGuess;

                return response.Solution;
            }

            public async Task<HangmanResponseStatus> Guess(char letter, CancellationToken cancellationToken = default)
            {
                try
                {
                    var content = new FormUrlEncodedContent(new Dictionary<string, string?>
                    {
                        ["token"] = Token,
                        ["letter"] = letter.ToString()
                    });
                    var response = await httpService.Send(ApiAddress,
                        HttpMethod.Put,
                        content,
                        cancellationToken: cancellationToken);

                    if (response == null)
                        return HangmanResponseStatus.Error;

                    // I decided to make a more raw way of dealing with HTTP
                    // since I needed the status code ffs.
                    if (response.StatusCode == HttpStatusCode.NotModified)
                        return HangmanResponseStatus.AlreadyGuessed;

                    var stream = await response.Content.ReadAsStreamAsync();
                    var dto = await JsonSerializer.DeserializeAsync<HangmanDto>(stream, 
                        cancellationToken: cancellationToken);

                    if (dto == null)
                        return HangmanResponseStatus.Error;

                    if (dto.Correct)
                    {
                        var newWord = dto.Hangman!;
                        if (newWord.Length != LastStatusWord?.Length)
                        {
                            Console.WriteLine("Hangman error. Word length mismatch!");
                            return HangmanResponseStatus.Error;
                        }

                        for(var i = 0; i < newWord.Length; ++i)
                        {
                            var c = newWord[i];
                            if (c == KnownPlaceholder) continue;
                            LastStatusWord = LastStatusWord?.Remove(i, 1).Insert(i, c.ToString());
                        }
                        RemainingLettersToGuess = LastStatusWord.Count(e => e != KnownPlaceholder);
                    }

                    // Finally!
                    return dto.Correct ? HangmanResponseStatus.Correct : HangmanResponseStatus.Wrong;
                }
                catch (ArgumentNullException) { return HangmanResponseStatus.Error; }
                catch (JsonException) { return HangmanResponseStatus.Error; }
                catch (NotSupportedException) { return HangmanResponseStatus.Error; }
                catch (HttpRequestException) { return HangmanResponseStatus.Error; }
            }

            public string? Token { get; private set; }
            public HangmanGameStatus Status { get; private set; }
            public int RemainingLettersToGuess { get; private set; }
            public int CountOfLettersToGuess { get; private set; }
            public string? Initiator { get; private set; }
            public string? LastStatusWord { get; set; }
            public string? SanitizedStatusWord => Sanitize(LastStatusWord);
            public double Progress => RemainingLettersToGuess / (double)CountOfLettersToGuess;
            public bool IsCompleted => Progress >= 1;
            public IReadOnlyDictionary<IUser, Score> Participants => participants;

            private static string? Sanitize(string? text)
            {
                return text?.Replace(KnownPlaceholder.ToString(), $" \\{KnownPlaceholder} ");
            }
        }

        // TODO comply with the specs!

        /* #Phase1 - creating the token for particular channel
         * !hangman start
         * bot: _draws the hangman figure with bunch of underscores below representing the guessed letters_
         * 
         * #Phase2 - guessing letters for this channel
         * - if #Phase1 hasn't been initiated first, this should produce an error
         * - and notify the users
         * !hangman a
         * 
         * #Phase3 - bot responses during guessing
         * !hangman a
         * bot: _the hangman figure should produce lines to represent a man being hanged
         * - little by little for every wrong guesses. For every correct guesses, the underscores below
         * - should reveal every pieces of the word._
         * 
         * #Phase4 - winning/losing the game
         * !hangman b
         * bot: _draws the final line or reveals the final letter below.
         * - when the user wins, bot will mention the name of the final guesser, each user points
         * - on the current session, and some emojis so that it doesn't look dull.
         * - when the user loses, display some dumb meme.
         * - automatically clear the resources for every win/lose status._
         * 
         * #Phase5 - Let's get started!
         * 
         * 
         * 
         * Problem: API doesn't save our guessed letters so we need to merge the following texts.
         * 
         *    _ a _ _ _ _ _ a _ a _
         * +  _ _ _ _ _ o _ _ _ _ _
         * =  _ a _ _ _ o _ a _ a _
         * 
         * 
         * var oldWord
         * var newWord
         * 
         * for (i ..newWord.indices) {
         *      let c = newWord[i]
         *      if (c == '_') continue;
         *      oldWord[i] = c;
         * }
         * 
         * TADAA!!!
         */
    }
}
