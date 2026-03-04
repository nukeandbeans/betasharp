using System.Collections;
using System.Text;

namespace BetaSharp.IO;

/// <summary>
/// This class reads Java style properties from an input stream.
/// </summary>
public class PropertyFileReader
{
    private const int MatchEndOfInput = 1;
    private const int MatchTerminator = 2;
    private const int MatchWhitespace = 3;
    private const int MatchAny = 4;

    private const int ActionAddToKey = 1;
    private const int ActionAddToValue = 2;
    private const int ActionStoreProperty = 3;
    private const int ActionEscape = 4;
    private const int ActionIgnore = 5;

    private const int StateStart = 0;
    private const int StateComment = 1;
    private const int StateKey = 2;
    private const int StateKeyEscape = 3;
    private const int StateKeyWs = 4;
    private const int StateBeforeSeparator = 5;
    private const int StateAfterSeparator = 6;
    private const int StateValue = 7;
    private const int StateValueEscape = 8;
    private const int StateValueWs = 9;
    private const int StateFinish = 10;

    private static readonly int[][] s_states =
    [
        [ //STATE_start
            MatchEndOfInput, StateFinish, ActionIgnore, MatchTerminator, StateStart, ActionIgnore, '#', StateComment, ActionIgnore, '!', StateComment, ActionIgnore, MatchWhitespace, StateStart, ActionIgnore, '\\', StateKeyEscape, ActionEscape, ':', StateAfterSeparator, ActionIgnore, '=', StateAfterSeparator, ActionIgnore, MatchAny, StateKey, ActionAddToKey
        ]
      , [ //STATE_comment
            MatchEndOfInput, StateFinish, ActionIgnore, MatchTerminator, StateStart, ActionIgnore, MatchAny, StateComment, ActionIgnore
        ]
      , [ //STATE_key
            MatchEndOfInput, StateFinish, ActionStoreProperty, MatchTerminator, StateStart, ActionStoreProperty, MatchWhitespace, StateBeforeSeparator, ActionIgnore, '\\', StateKeyEscape, ActionEscape, ':', StateAfterSeparator, ActionIgnore, '=', StateAfterSeparator, ActionIgnore, MatchAny, StateKey, ActionAddToKey
        ]
      , [ //STATE_key_escape
            MatchTerminator, StateKeyWs, ActionIgnore, MatchAny, StateKey, ActionAddToKey
        ]
      , [ //STATE_key_ws
            MatchEndOfInput, StateFinish, ActionStoreProperty, MatchTerminator, StateStart, ActionStoreProperty, MatchWhitespace, StateKeyWs, ActionIgnore, '\\', StateKeyEscape, ActionEscape, ':', StateAfterSeparator, ActionIgnore, '=', StateAfterSeparator, ActionIgnore, MatchAny, StateKey, ActionAddToKey
        ]
      , [ //STATE_before_separator
            MatchEndOfInput, StateFinish, ActionStoreProperty, MatchTerminator, StateStart, ActionStoreProperty, MatchWhitespace, StateBeforeSeparator, ActionIgnore, '\\', StateValueEscape, ActionEscape, ':', StateAfterSeparator, ActionIgnore, '=', StateAfterSeparator, ActionIgnore, MatchAny, StateValue, ActionAddToValue
        ]
      , [ //STATE_after_separator
            MatchEndOfInput, StateFinish, ActionStoreProperty, MatchTerminator, StateStart, ActionStoreProperty, MatchWhitespace, StateAfterSeparator, ActionIgnore, '\\', StateValueEscape, ActionEscape, MatchAny, StateValue, ActionAddToValue
        ]
      , [ //STATE_value
            MatchEndOfInput, StateFinish, ActionStoreProperty, MatchTerminator, StateStart, ActionStoreProperty, '\\', StateValueEscape, ActionEscape, MatchAny, StateValue, ActionAddToValue
        ]
      , [ //STATE_value_escape
            MatchTerminator, StateValueWs, ActionIgnore, MatchAny, StateValue, ActionAddToValue
        ]
      , [ //STATE_value_ws
            MatchEndOfInput, StateFinish, ActionStoreProperty, MatchTerminator, StateStart, ActionStoreProperty, MatchWhitespace, StateValueWs, ActionIgnore, '\\', StateValueEscape, ActionEscape, MatchAny, StateValue, ActionAddToValue
        ]
    ];

    private readonly Hashtable _hashtable;

    private const int BufferSize = 1000;

    private bool _escaped;
    private readonly StringBuilder _keyBuilder = new();
    private readonly StringBuilder _valueBuilder = new();

    /// <summary>
    /// Construct a reader passing a reference to a Hashtable (or JavaProperties) instance
    /// where the keys are to be stored.
    /// </summary>
    /// <param name="hashtable">A reference to a hashtable where the key-value pairs can be stored.</param>
    public PropertyFileReader(Hashtable hashtable)
    {
        _hashtable = hashtable;
    }

    /// <summary>
    /// <para>Load key value pairs (properties) from an input Stream expected to have ISO-8859-1 encoding (code page 28592).
    /// The input stream (usually reading from a ".properties" file) consists of a series of lines (terminated
    /// by \r, \n or \r\n) each a key value pair, a comment or a blank line.</para>
    ///
    /// <para>Leading whitespace (spaces, tabs, formfeeds) are ignored at the start of any line - and a line that is empty or
    /// contains only whitespace is blank and ignored.</para>
    ///
    /// <para>A line with the first non-whitespace character is a '#' or '!' is a comment line and the rest of the line is
    /// ignored.</para>
    ///
    /// <para>If the first non-whitespace character is not '#' or '!' then it is the start of a key.  A key is all the
    /// characters up to the first whitespace or a key/value separator - '=' or ':'.</para>
    ///
    /// <para>The separator is optional.  Any whitespace after the key or after the separator (if present) is ignored.</para>
    ///
    /// <para>The first non-whitespace character after the separator (or after the key if no separator) begins the value.
    /// The value may include whitespace, separators, or comment characters.</para>
    ///
    /// <para>Any unicode character may be included in either key or value by using escapes preceded by the escape
    /// character '\'.</para>
    ///
    /// <para>The following special cases are defined:</para>
    /// <code>
    /// 	'\t' - horizontal tab.
    /// 	'\f' - form feed.
    /// 	'\r' - return
    /// 	'\n' - new line
    /// 	'\\' - add escape character.
    ///
    /// 	'\ ' - add space in a key or at the start of a value.
    /// 	'\!', '\#' - add comment markers at the start of a key.
    /// 	'\=', '\:' - add a separator in a key.
    /// </code>
    ///
    /// <para>Any unicode character using the following escape:</para>
    /// <code>
    /// 	'\uXXXX' - where XXXX represents the unicode character code as 4 hexadecimal digits.
    /// </code>
    ///
    /// <para>Finally, longer lines can be broken by putting an escape at the very end of the line.  Any leading space
    /// (unless escaped) is skipped at the beginning of the following line.</para>
    ///
    /// Examples
    /// <code>
    /// 	a-key = a-value
    /// 	a-key : a-value
    /// 	a-key=a-value
    /// 	a-key a-value
    /// </code>
    ///
    /// <para>All the above will result in the same key/value pair - key "a-key" and value "a-value".</para>
    /// <code>
    /// 	! comment...
    /// 	# another comment...
    /// </code>
    ///
    /// <para>The above are two examples of comments.</para>
    /// <code>
    /// 	Honk\ Kong = Near China
    /// </code>
    ///
    /// <para>The above shows how to embed a space in a key - key is "Hong Kong", value is "Near China".</para>
    /// <code>
    /// 	a-longer-key-example = a really long value that is \
    /// 			split over two lines.
    /// </code>
    ///
    /// <para>An example of a long line split into two.</para>
    /// </summary>
    /// <param name="stream">The input stream that the properties are read from.</param>
    /// <param name="encoding">The <see cref="System.Text.Encoding">encoding</see> that is used to read the properies file stream.</param>
    public void Parse(Stream stream, Encoding encoding = null)
    {
        var bufferedStream = new BufferedStream(stream, BufferSize);
        // the default encoding ISO-8859-1 (codepabe 28591) will be used if we do not pass explicitly different encoding
        var parserEncoding = encoding ?? PropertiesFile.DefaultEncoding;
        _reader = new BinaryReader(bufferedStream, parserEncoding);

        int state = StateStart;

        do
        {
            int ch = NextChar();

            bool matched = false;

            for (int s = 0; s < s_states[state].Length; s += 3)
            {
                if (Matches(s_states[state][s], ch))
                {
                    //Debug.WriteLine( stateNames[ state ] + ", " + (s/3) + ", " + ch + (ch>20?" (" + (char) ch + ")" : "") );
                    matched = true;
                    DoAction(s_states[state][s + 2], ch);

                    state = s_states[state][s + 1];

                    break;
                }
            }

            if (!matched)
            {
                throw new Exception($"Unexpected character at {1}: <<<{ch}>>>");
            }
        }
        while (state != StateFinish);
    }

    private bool Matches(int match, int ch)
    {
        switch (match)
        {
            case MatchEndOfInput:
                return ch == -1;

            case MatchTerminator:
                if (ch == '\r')
                {
                    if (PeekChar() == '\n')
                    {
                        _saved = false;
                    }

                    return true;
                }

                if (ch == '\n')
                {
                    return true;
                }

                return false;

            case MatchWhitespace:
                return ch == ' ' || ch == '\t' || ch == '\f';

            case MatchAny:
                return true;

            default:
                return ch == match;
        }
    }

    private void DoAction(int action, int ch)
    {
        switch (action)
        {
            case ActionAddToKey:
                _keyBuilder.Append(EscapedChar(ch));
                _escaped = false;

                break;

            case ActionAddToValue:
                _valueBuilder.Append(EscapedChar(ch));
                _escaped = false;

                break;

            case ActionStoreProperty:
                //Debug.WriteLine( keyBuilder.ToString() + "=" + valueBuilder.ToString() );
                // Corrected to avoid duplicate entry errors - thanks to David Tanner.
                _hashtable[_keyBuilder.ToString()] = _valueBuilder.ToString();
                _keyBuilder.Length = 0;
                _valueBuilder.Length = 0;
                _escaped = false;

                break;

            case ActionEscape:
                _escaped = true;

                break;

            //case ACTION_ignore:
            default:
                _escaped = false;

                break;
        }
    }

    private char EscapedChar(int ch)
    {
        if (_escaped)
        {
            switch (ch)
            {
                case 't':
                    return '\t';
                case 'r':
                    return '\r';
                case 'n':
                    return '\n';
                case 'f':
                    return '\f';
                case 'u':
                    int uch = 0;

                    for (int i = 0; i < 4; i++)
                    {
                        ch = NextChar();

                        uch = ch switch
                        {
                            >= '0' and <= '9' => (uch << 4) + ch - '0'
                          , >= 'a' and <= 'z' => (uch << 4) + ch - 'a' + 10
                          , >= 'A' and <= 'Z' => (uch << 4) + ch - 'A' + 10
                          , _                 => throw new Exception("Invalid Unicode character.")
                        };
                    }

                    return (char)uch;
            }
        }

        return (char)ch;
    }

    // we now use a BinaryReader, which supports encodings
    private BinaryReader _reader;
    private int _savedChar;
    private bool _saved;

    private int NextChar()
    {
        if (_saved)
        {
            _saved = false;

            return _savedChar;
        }

        return ReadCharSafe();
    }

    private int PeekChar()
    {
        if (_saved)
        {
            return _savedChar;
        }

        _saved = true;

        return _savedChar = ReadCharSafe();
    }

    /// <summary>
    /// A method to substitute calls to <c>stream.ReadByte()</c>.
    /// The <see cref="PropertyFileReader" /> now uses a <see cref="BinaryReader"/> to read properties.
    /// Unlike a plain stream, the <see cref="BinaryReader"/> will not return -1 when the stream end is reached,
    /// instead an <see cref="IOException" /> is to be thrown.
    /// <para>
    /// In this method we perform a check if the stream is already processed to the end, and return <c>-1</c>.
    /// </para>
    /// </summary>
    /// <returns></returns>
    private int ReadCharSafe()
    {
        if (_reader.BaseStream.Position == _reader.BaseStream.Length)
        {
            // We have reached the end of the stream. The reder will throw exception if we call Read any further.
            // We just return -1 now;
            return -1;
        }

        // reader.ReadChar() will take into account the encoding.
        return _reader.ReadChar();
    }
}
