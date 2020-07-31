using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace brainfsck
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new BrainfuckIntepreter { Reader = File.OpenText(args[0]) }.Start();
        }
    }

    /// <summary>
    /// Tokens that'd be generated upon reading a Brainfuck file.
    /// </summary>
    public enum TokenType
    {
        Increment    = '+',
        Decrement    = '-',
        Forward      = '>',
        Backward     = '<',
        PutCharacter = '.',
        GetCharacter = ','
    };

    /// <summary>
    /// Thrown if a Brainfuck token was not recognized.
    /// </summary>
    public class InvalidBrainfuckToken : Exception
    {
        public InvalidBrainfuckToken(char token) : base(token.ToString()) { }
    }

    /// <summary>
    /// Inteprets Brainfuck tokens and executes them immedieately.
    /// </summary>
    public class BrainfuckIntepreter
    {
        public TextReader Reader { get; set; }

        internal class Looper
        {
            public List<TokenType> tokens;
            public int tokenIndex = 0;
            public bool active = false;
        }

        internal class Executor
        {
            public List<char> memory = new List<char>(new char[]{ '\0' });
            public int memoryIndex = 0;
        }

        private Executor executor = new Executor();
        private List<Looper> loopStates = new List<Looper>();
        private int loopStateIndex = 0;

        /// <summary>
        /// Starts the execution by pulling data from the input stream.
        /// </summary>
        public void Start()
        {
            char? operation = null;
            var loopState = loopStates.ElementAtOrDefault(loopStateIndex);
            bool isLoopActive = loopState?.active ?? false;
            
            if ( !isLoopActive )
            {
                int op = Reader.Read();
                if ( op != -1 )
                    operation = (char?) op;
                else return;
            }

            if ( operation != null && "+-><.,[]".IndexOf((char) operation) == -1 )
                throw new InvalidBrainfuckToken((char) operation);

            if ( operation == '[' )
            {
                loopStates.Add(loopState = new Looper
                {
                    active = true,
                    tokens = Utils.ReadUntil(Reader,']')
                                  .Select(op => (TokenType) op)
                                  .ToList()
                });
                
                ++loopStateIndex;
                isLoopActive = true;
            }

            if ( isLoopActive )
            {
                Execute((TokenType)loopState?.tokens[loopState.tokenIndex]);
                loopState.tokenIndex++;
                loopState.tokenIndex %= loopState.tokens.Count;

                // as of the brainfuck conventions, only quit looping unless,
                // the current memory slot's value becomes zero.
                if ( executor.memory[executor.memoryIndex] == 0 )
                    loopState.active = false;
            }
            else Execute((TokenType) operation);

            Start();  // iterate all the operations through recursion.
        }

        public void Execute(TokenType token)
        {
            switch (token)
            {
                case TokenType.Increment:
                    executor.memory[executor.memoryIndex] +=
                        executor.memoryIndex == 255 ? '\0' : '\x01'; break;
                
                case TokenType.Decrement:
                    executor.memory[executor.memoryIndex] -=
                        executor.memoryIndex == 0 ? '\0' : '\x01'; break;

                case TokenType.Forward:
                    if (executor.memory.Count >= executor.memoryIndex)
                        executor.memory.Add('\0');
                    ++executor.memoryIndex;
                    break;

                case TokenType.Backward:
                    executor.memoryIndex -= 
                        executor.memoryIndex == 0 ? '\0' : '\x01'; break;

                case TokenType.PutCharacter: 
                    Console.WriteLine((int)executor.memory[executor.memoryIndex]); break;

                case TokenType.GetCharacter:
                    executor.memory[executor.memoryIndex] = (char)Console.Read(); break;
            }
        }
    }
}
