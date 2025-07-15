using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DP.Base.ComponentModel.Serialization
{
    public static class SerializationUtils
    {
        public static byte[] GetBytes(decimal dec)
        {
            // Load four 32 bit integers from the Decimal.GetBits function
            int[] bits = decimal.GetBits(dec);
            // Create a temporary list to hold the bytes
            List<byte> bytes = new List<byte>();
            // iterate each 32 bit integer
            foreach (int i in bits)
            {
                //add the bytes of the current 32bit integer
                //to the bytes list
                bytes.AddRange(BitConverter.GetBytes(i));
            }

            //return the bytes list as an array
            return bytes.ToArray();
        }

        public static decimal ToDecimal(byte[] bytes)
        {
            // check that it is even possible to convert the array
            if (bytes.Count() < 16)
            {
                throw new Exception("A decimal must be created from exactly 16 bytes");
            }

            // make an array to convert back to int32's
            int[] bits = new int[4];
            for (int i = 0; i <= 15; i += 4)
            {
                //convert every 4 bytes into an int32
                bits[i / 4] = BitConverter.ToInt32(bytes, i);
            }

            // Use the decimal's new constructor to
            // create an instance of decimal
            return new decimal(bits);
        }

        public static void ReadBytesToBuffer(Stream stream, byte[] buffer, int numberToRead)
        {
            if (stream.CanTimeout)
            {
                stream.ReadTimeout = 1000 * 60 * 5;
            }

            int readCount = 0;
            int remaining = numberToRead;
            while (remaining > 0)
            {
                if (stream.Position == stream.Length)
                {
                    throw new InvalidDataException("Unexpected end of stream!");
                }

                int tmpReadCount = stream.Read(buffer, readCount, remaining);
                remaining -= tmpReadCount;
                readCount += tmpReadCount;
            }
        }
    }
}
