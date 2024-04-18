using System.Collections;
using System.Drawing;
using System.IO;

namespace Process
{
    internal class Program
    {
        #region Constants
        const String FileLocation = "../../../Commands.txt";
        const int Size = 26;
        #endregion

        #region Technical
        enum Register { R1, R2, R3};
        static List<String[]> buffer = [];
        static Dictionary<String, Register> registerDict = new() {
            { "R1", Register.R1 },
            { "R2", Register.R2 },
            { "R3", Register.R3 }
        };
        #endregion

        #region Registers
        static BitArray[] registers = [
            new(Size, false),
            new(Size, false),
            new(Size, false),
        ];      
        static bool PS = false;
        static int PC = 0;
        static int TC = 0;
        #endregion

        static void Main(string[] args)
        {
            ReadCommandFile(FileLocation);
            Console.WriteLine();
            Execute();
            
        }
        #region Technical Commands
        static void PrintAll(String[] command)
        {
            var com = String.Join(" ", command);
            Console.WriteLine("Command: " + com);
            int i = 1;
            foreach(var reg in registers)
            {
                Console.Write($"R{i}: ");
                i++;
                PrintBinaryArray(reg);
                Console.WriteLine();
            }
            var sign = PS ? "-" : "+";
            Console.WriteLine($"PS: {sign}");
            Console.WriteLine($"PC: {PC}");
            Console.WriteLine($"TC: {TC}");
            Console.WriteLine();
        }
        static void PrintBinaryArray(IEnumerable<bool> array)
        {
            foreach(var i in array)
            {
                Console.Write(i ? "1":"0");
            }
        }
        static void PrintBinaryArray(BitArray array)
        {
            int j = 1;
            foreach (bool i in array)
            {
                if (j % 4 == 0) Console.Write(" ");
                Console.Write(i ? "1" : "0");
                j++;

            }
        }
        static void ReadCommandFile(string path)
        {
            String? line;
            try
            {
                StreamReader sr = new(path);
                line = sr.ReadLine();
                while (line != null)
                {
                    buffer.Add(ReadCommand(line));
                    Console.WriteLine(line);
                    line = sr.ReadLine();                    
                }
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }
        static String[] ReadCommand(string command)
        {
            return command.Split(' ');
        }
        static List<bool> ToBinary(int value)
        {
            List<bool> result = [];
            int a = Math.Abs(value);
            while(a > 0)
            {
                result.Add(a % 2 == 1);
                a /= 2;
            }
            result.Reverse();    
            return result;
        }         
        static Point GetPositions(Register register)
        {
            int pos = GetIntFromBitArray(registers[(int)register]);
            return GetPositions(pos);
        }
        static Point GetPositions(int pos)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(pos, Size);
            int x, y;
            y = pos % 100;
            pos /= 100;
            x = pos;

            return new Point(x, y);
        }
        static int GetIntFromBitArray(BitArray bitArray)
        {
            if (bitArray[0] == true)
            {
                bitArray = bitArray.Not();
                bool k = true;
                int i = 1;
                while (k && i <= Size)
                {
                    bitArray[^i] ^= k;
                    if (bitArray[^i]) k = false;
                    i++;
                }
            }
            int value = 0;
            //start of number 
            int j = 0;

            for (int i = 1; i <= bitArray.Count; i++)
            {                
                if (bitArray[^i])
                {
                    value += Convert.ToInt32(Math.Pow(2, j));
                }
                j++;
            }

            return value;
        }
        static void Execute()
        {
            foreach (var command in buffer)
            {
                PC++;
                TC++;
                PrintAll(command);
#warning Remove Comment
                //Console.Readline();
                if (!registerDict.ContainsKey(command[1]))
                {
                    Console.WriteLine("First operand must be register");
                    return;
                }
                var writeIn = registerDict[command[1]];

                if (int.TryParse(command[2], out int secondOperand))
                {
                    switch (command[0])
                    {
                        case "MOV":
                            if (command.Length != 3)
                            {
                                Console.WriteLine("Invalid arguments");
                                return;
                            }
                            MOV(writeIn, secondOperand);
                            break;
                        case "STP":
                            if (command.Length == 4)
                            {
                                if (int.TryParse(command[3], out int thirdOperand))
                                {
                                    STP(writeIn, secondOperand, thirdOperand);
                                }
                                else if (registerDict.TryGetValue(command[3], out Register value))
                                {
                                    STP(writeIn, secondOperand, value);
                                }
                                else { Console.WriteLine("Invalid arguments"); return; }
                            }
                            else
                            {
                                Console.WriteLine("Invalid arguments");
                                return;
                            }
                            break;
                        case "SWP":

                            if (command.Length == 4)
                            {
                                if (int.TryParse(command[3], out int thirdOperand))
                                {
                                    SWP(writeIn, secondOperand, thirdOperand);
                                }
                                else if (registerDict.TryGetValue(command[3], out Register value))
                                {
                                    SWP(writeIn, secondOperand, value);
                                }
                                else { Console.WriteLine("Invalid arguments"); return; }
                            }
                            else if (command.Length == 3)
                            {
                                SWP(writeIn, secondOperand);
                            }
                            else
                            {
                                Console.WriteLine("No valid arguments");
                                return;
                            }
                            break;

                        default:
                            Console.WriteLine("Command was not found");
                            return;

                    }
                }
                else if (registerDict.TryGetValue(command[2], out Register value))
                {
                    Register register = value;
                    switch (command[0])
                    {
                        case "MOV":
                            if (command.Length != 3)
                            {
                                Console.WriteLine("Invalid arguments");
                                return;
                            }
                            MOV(writeIn, register);
                            break;
                        case "STP":
                            if (command.Length == 4)
                            {
                                if (int.TryParse(command[3], out int thirdOperand))
                                {
                                    STP(writeIn, register, thirdOperand);
                                }
                                else if (registerDict.TryGetValue(command[3], out Register reg))
                                {
                                    STP(writeIn, register, reg);
                                }
                                else { Console.WriteLine("Invalid arguments"); return; }
                            }
                            else
                            {
                                Console.WriteLine("Invalid arguments");
                                return;
                            }
                            break;
                        case "SWP":
                            if (command.Length == 4)
                            {
                                if (int.TryParse(command[3], out int thirdOperand))
                                {
                                    SWP(writeIn, register, thirdOperand);
                                }
                                else if (registerDict.TryGetValue(command[3], out Register reg))
                                {
                                    SWP(writeIn, register, reg);
                                }
                                else { Console.WriteLine("Invalid arguments"); return; }
                            }
                            else if (command.Length == 3)
                            {
                                SWP(writeIn, register);
                            }
                            else
                            {
                                Console.WriteLine("No valid arguments");
                                return;
                            }
                            break;

                        default:
                            Console.WriteLine("Command was not found");
                            return;

                    }
                }
                TC++;
                PS = registers[(int)writeIn][0];
                PrintAll(command);
                TC = 0;

#warning Remove comment
                //Console.ReadLine();
            }
        }
        #endregion

        #region Commands
        //plave value into register
        static void MOV(Register writeInto, int value)
        {
            var array = ToBinary(value);
            for(int i = 1; i <= array.Count; i++)
            {
                registers[(int)writeInto][^i] = array[^i];
            }
            if (value < 0)
            {
                registers[(int)writeInto] = registers[(int)writeInto].Not();
                bool k = true;
                int i = 1;
                while(k && i <= Size)
                {
                    registers[(int)writeInto][^i] ^= k;
                    if (registers[(int)writeInto][^i]) k = false;
                    i++;                    
                }            
            }
        }
        static void MOV(Register writeInto, Register writeOut)
        {
            registers[(int)writeInto] = registers[(int)writeOut];
        }
        //set position of bits in register
        static void STP(Register writeInto, int x, int y)
        {            
            if (x > Size || y > Size || x < 1 || y < 1) 
                throw new ArgumentOutOfRangeException($"Positions must be between 1 and {Size}");
            if (y < 10) x *= 10;
            var pos = ToBinary(x * 100 + y);
                        
            for (int i = 1; i <= pos.Count; i++)
            {
                registers[(int)writeInto][^i] = pos[^i];
            }
        }
        static void STP(Register writeInto, int x, Register pos2)
        {
            int y = GetIntFromBitArray (registers[(int)pos2]);
            if (x > Size || y > Size || x < 1 || y < 1)
                throw new ArgumentOutOfRangeException($"Positions must be between 1 and {Size}");
            if (y < 10) x *= 10;
            var pos = ToBinary(x * 100 + y);

            for (int i = 1; i <= pos.Count; i++)
            {
                registers[(int)writeInto][^i] = pos[^i];
            }
        }
        static void STP(Register writeInto, Register pos1, int y)
        {
            int x = GetIntFromBitArray(registers[(int)pos1]);
            if (x > Size || y > Size || x < 1 || y < 1)
                throw new ArgumentOutOfRangeException($"Positions must be between 1 and {Size}");
            if (y < 10) x *= 10;
            var pos = ToBinary(x * 100 + y);

            for (int i = 1; i <= pos.Count; i++)
            {
                registers[(int)writeInto][^i] = pos[^i];
            }
        }
        static void STP(Register writeInto, Register x, Register y)
        {
            SWP(writeInto, GetIntFromBitArray(registers[(int)x]), GetIntFromBitArray(registers[(int)y]));
        }
        //swap Bits
        static void SWP(Register writeInto, Register pos)
        {
            SWP(writeInto, GetIntFromBitArray(registers[(int)pos]));
        }
        static void SWP(Register writeInto, int pos)
        {
            if (pos < 0)
            {
                Console.WriteLine("Negative operands");
                return;
            }
            var itemsPos = GetPositions(pos);
            (registers[(int)writeInto][itemsPos.Y], registers[(int)writeInto][itemsPos.X]) = 
                (registers[(int)writeInto][itemsPos.X], registers[(int)writeInto][itemsPos.Y]);
        }
        static void SWP(Register writeInto, int x, int y) {
            if (x < 0 || y < 0 ||
                x > Size || y > Size)
            {
                Console.WriteLine("Negative operands");
                return;
            }
            (registers[(int)writeInto][x], registers[(int)writeInto][y]) =
                (registers[(int)writeInto][y], registers[(int)writeInto][x]);
        }
        static void SWP(Register writeInto, int x, Register pos2)
        {
            int y = GetIntFromBitArray(registers[(int)pos2]);
            if (x < 0 || y < 0 ||
                x > Size || y > Size)
            {
                Console.WriteLine("Negative operands");
                return;
            }
            (registers[(int)writeInto][x], registers[(int)writeInto][y]) =
                (registers[(int)writeInto][y], registers[(int)writeInto][x]);
        }
        static void SWP(Register writeInto, Register pos1, int y)
        {
            int x = GetIntFromBitArray(registers[(int)pos1]);
            if (x < 0 || y < 0 ||
                x > Size || y > Size)
            {
                Console.WriteLine("Negative operands");
                return;
            }
            (registers[(int)writeInto][x], registers[(int)writeInto][y]) =
                (registers[(int)writeInto][y], registers[(int)writeInto][x]);
        }
        static void SWP(Register writeInto, Register pos1, Register pos2)
        {
            int x = GetIntFromBitArray(registers[(int)pos1]);
            int y = GetIntFromBitArray(registers[(int)pos2]);
            if (x < 0 || y < 0 ||
                x > Size || y > Size)
            {
                Console.WriteLine("Negative operands");
                return;
            }
            (registers[(int)writeInto][x], registers[(int)writeInto][y]) =
                (registers[(int)writeInto][y], registers[(int)writeInto][x]);
        }

        #endregion
    }
}
