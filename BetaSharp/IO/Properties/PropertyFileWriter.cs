using System.Collections;
using System.Text;

namespace BetaSharp.IO;

/// <summary>
/// Use this class for writing a set of key value pair strings to an
/// output stream using the Java properties format.
/// </summary>
public class PropertyFileWriter
{
    private static readonly char[] s_hex = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'];

    private readonly Hashtable _hashtable;

    /// <summary>
    /// Construct an instance of this class.
    /// </summary>
    /// <param name="hashtable">The Hashtable (or JavaProperties) instance
    /// whose values are to be written.</param>
    public PropertyFileWriter(Hashtable hashtable)
    {
        _hashtable = hashtable;
    }

    /// <summary>
    /// Write the properties to the output stream.
    /// </summary>
    /// <param name="stream">The output stream where the properties are written.</param>
    /// <param name="comments">Optional comments that are placed at the beginning of the output.</param>
    public void Write(Stream stream, string comments)
    {
        //  Create a writer to output to an ISO-8859-1 encoding (code page 28591).
        StreamWriter writer = new( stream, Encoding.Latin1 );

        if (comments != null)
        {
            writer.WriteLine($"# {comments}");
        }

        writer.WriteLine($"# {DateTime.Now}");

        for (IEnumerator e = _hashtable.Keys.GetEnumerator(); e.MoveNext();)
        {
            string key = e.Current.ToString();
            string val = _hashtable[key].ToString();

            writer.WriteLine($"{EscapeKey(key)}={EscapeValue(val)}");
        }

        writer.Flush();
    }

    /// <summary>
    /// Escape the string as a Key with character set ISO-8859-1 -
    /// the characters 0-127 are US-ASCII and we will escape any others. The passed string is Unicode which extends
    /// ISO-8859-1 - so all is well.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private string EscapeKey(string s)
    {
        StringBuilder buf = new();
        bool first = true;

        foreach (char c in s)
        {
            //  Avoid confusing with a comment: '!' (33), '#' (35).
            if (first)
            {
                first = false;

                if (c == '!' || c == '#')
                {
                    buf.Append('\\');
                }
            }

            switch (c)
            {
                case '\t': //  =09 U+0009  HORIZONTAL TABULATION \t
                    buf.Append('\\').Append('t');

                    break;
                case '\n': //  =0A U+000A  LINE FEED \n
                    buf.Append('\\').Append('n');

                    break;
                case '\f': //  =0C U+000C  FORM FEED \f
                    buf.Append('\\').Append('f');

                    break;
                case '\r': //  =0D U+000D  CARRIAGE RETURN \r
                    buf.Append('\\').Append('r');

                    break;

                case ' ':  //  32: ' '
                case ':':  //  58: ':'
                case '=':  //  61: '='
                case '\\': //  92: '\'
                    buf.Append('\\').Append(c);

                    break;

                default:
                    if (c > 31 && c < 127)
                    {
                        buf.Append(c);
                    }
                    else
                    {
                        buf.Append('\\').Append('u');
                        buf.Append(s_hex[(c >> 12) & 0xF]);
                        buf.Append(s_hex[(c >> 8) & 0xF]);
                        buf.Append(s_hex[(c >> 4) & 0xF]);
                        buf.Append(s_hex[c & 0xF]);
                    }

                    break;
            }
        }

        return buf.ToString();
    }

    private string EscapeValue(string s)
    {
        StringBuilder buf = new();
        bool first = true;

        foreach (char c in s)
        {
            //  Handle value starting with whitespace.
            if (first)
            {
                first = false;

                switch( c ) {
                    case ' ':
                        buf.Append('\\').Append(' ');

                        continue;
                    //  =09 U+0009  HORIZONTAL TABULATION   \t
                    case '\t':
                        buf.Append('\\').Append('t');

                        continue;
                }
            }

            switch (c)
            {
                case '\t':            //  =09 U+0009  HORIZONTAL TABULATION \t
                    buf.Append('\t'); //OK after first position.

                    break;
                case '\n': //  =0A U+000A  LINE FEED \n
                    buf.Append('\\').Append('n');

                    break;
                case '\f': //  =0C U+000C  FORM FEED \f
                    buf.Append('\\').Append('f');

                    break;
                case '\r': //  =0D U+000D  CARRIAGE RETURN \r
                    buf.Append('\\').Append('r');

                    break;
                case '\\': //  92: '\'
                    buf.Append('\\').Append(c);

                    break;

                default:
                    if (c > 31 && c < 127)
                    {
                        buf.Append(c);
                    }
                    else
                    {
                        buf.Append('\\').Append('u');
                        buf.Append(s_hex[(c >> 12) & 0xF]);
                        buf.Append(s_hex[(c >> 8) & 0xF]);
                        buf.Append(s_hex[(c >> 4) & 0xF]);
                        buf.Append(s_hex[c & 0xF]);
                    }

                    break;
            }
        }

        return buf.ToString();
    }
}
