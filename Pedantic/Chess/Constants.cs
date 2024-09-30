// <copyright file="Constants.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    public static class Constants
    {
        public const int MAX_SQUARES = 64;
        public const int MAX_COLORS = 2;
        public const int MAX_PIECES = 6;
        public const int MAX_COORDS = 8;
        public const int MAX_DIRECTIONS = 8;
        public const int MAX_PLY = 96;
        public const int MAX_GAME_LENGTH = 768 + MAX_PLY;
        public const int MAX_PHASE = 64;
        public const int MAX_KING_BUCKETS = 16;
        public const int CHECKMATE_SCORE = 20000;
        public const int TABLEBASE_WIN = 19500;
        public const int TABLEBASE_LOSS = -19500;
        public const int MIN_TABLEBASE_WIN = TABLEBASE_WIN - MAX_PLY;
        public const int MAX_TABLEBASE_LOSS = TABLEBASE_LOSS + MAX_PLY;
        public const int NO_SCORE = short.MinValue;
        public const int HISTORY_SCORE_MAX = 8192;
        public const int HISTORY_SCORE_MIN = -HISTORY_SCORE_MAX;
        public const int PROMOTE_BONUS = 50000;
        public const int CAPTURE_BONUS = 60000;
        public const int PV_BONUS = 70000;
        public const int INFINITE_WINDOW = short.MaxValue;
        public const ulong BB_ALL = 0xfffffffffffffffful;
        public const ulong BB_NONE = 0;
        public const StringSplitOptions TOKEN_OPTIONS = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;

        public const string REGEX_FEN = @"^\s*([rnbqkpRNBQKP1-8]+/){7}[rnbqkpRNBQKP1-8]+\s[bw]\s(-|K?Q?k?q?)\s(-|[a-h][36])\s\d+\s\d+\s*$";
        public const string REGEX_MOVE = @"^[a-h][1-8][a-h][1-8](n|b|r|q)?$";
        public const string REGEX_INDEX = @"^[a-h][1-8]$";
        public const string FEN_EMPTY = @"8/8/8/8/8/8/8/8 w - - 0 0";
        public const string FEN_START_POS = @"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public const string APP_NAME = "Pedantic";
        public const string APP_VERSION = "2.1.0";
        public const string APP_NAME_VER = APP_NAME + " " + APP_VERSION;
        public const string APP_AUTHOR = "JoAnn D. Peeler";
        public const string PROGRAM_URL = "https://github.com/JoAnnP38/PedanticRF";

        public static readonly char[] CMD_SEP = [' ', '\t'];
    }
}
