using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace brainfsck
{
    public class Utils
    {
        /// <summary>
        /// Read a string from a TextReader until a certain delimeter is reached.
        /// </summary>
        /// <param name="reader">object to read from</param>
        /// <param name="delimeter">delimeter to look for</param>
        /// <returns></returns>
        public static string ReadUntil(TextReader reader, char delimeter)
        {
            var text = new StringBuilder();
            for (var current = (char)reader.Read(); current != delimeter; current = (char)reader.Read())
                text.Append(current);

            return text.ToString();
        }
    }
}
