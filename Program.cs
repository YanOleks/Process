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
        enum Register { R1, R2 };
        static List<String[]> buffer = [];
        #endregion

        #region Registers
        static BitArray[] registers = [
            new(Size, false),
            new(Size, false),
        ];      
        static bool PS = false;
        static int PC = 0;
        static int TC = 0;
        #endregion

        static void Main(string[] args)
        {
            STP(Register.R1, 10, 14);
            Point point = GetPositions(Register.R1);
            Console.Write($"{point.X} {point.Y}");
            
        }
        #region Technical Commands
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
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
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
            int pos = getIntFromBitArray(registers[(int)register]);
            return GetPositions(pos);
        }
        static Point GetPositions(int pos)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(pos, 2626);
            int x, y;
            y = pos % 100;
            pos /= 100;
            x = pos;

            return new Point(x, y);
        }
        static int getIntFromBitArray(BitArray bitArray)
        {
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
#warning Not implemented
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
                registers[(int)writeInto].Not();
                bool k = true;
                int i = 1;
                while(k && i <= 26)
                {
                    registers[(int)writeInto][^i] ^= k;
                    i++;
                    if (registers[(int)writeInto][^i]) k = false;
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
        //swap Bits
        static void SWP(Register writeInto, Register pos)
        {
            SWP(writeInto, getIntFromBitArray(registers[(int)pos]));
        }
        static void SWP(Register writeInto, int pos)
        {
            var itemsPos = GetPositions(pos);
            (registers[(int)writeInto][itemsPos.Y], registers[(int)writeInto][itemsPos.X]) = 
                (registers[(int)writeInto][itemsPos.X], registers[(int)writeInto][itemsPos.Y]);
        }

        #endregion
    }
}
