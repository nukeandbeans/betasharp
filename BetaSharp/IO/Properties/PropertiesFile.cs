using System.Collections;
using System.Globalization;
using System.Text;

namespace BetaSharp.IO;

/// <summary>
/// Hold Java style properties as key-value pairs and allow them to be loaded from or
/// saved to a ".properties" file. The file is stored with character set ISO-8859-1 which extends US-ASCII
/// (the characters 0-127 are the same) and forms the first part of the Unicode character set.  Within the
/// application <see cref="string"/> are Unicode - but all values outside the basic US-ASCII set are escaped.
/// </summary>
public class PropertiesFile : Hashtable
{
    /// <summary>
    /// Gets a reference to the ISO-8859-1 encoding (code page 28591). This is the Java standard for .properties files.
    /// </summary>
    internal static Encoding DefaultEncoding
    {
        get { return Encoding.Latin1; }
    }

    /// <summary>
    /// A reference to an optional set of default properties - these values are returned
    /// if the value has not been loaded from a ".properties" file or set programatically.
    /// </summary>
    protected readonly Hashtable defaults;

    /// <summary>
    /// An empty constructor that doesn't set the defaults.
    /// </summary>
    public PropertiesFile()
    {
        defaults = new Hashtable();
    }

    /// <summary>
    /// Use this constructor to provide a set of default values.  The default values are kept separate
    /// to the ones in this instant.
    /// </summary>
    /// <param name="defaults">A Hashtable that holds a set of defafult key value pairs to
    /// return when the requested key has not been set.</param>
    public PropertiesFile(Hashtable defaults)
    {
        this.defaults = defaults;
    }

    /// <summary>
    /// Load Java Properties from a stream expecting the format as described in <see cref="PropertyFileReader"/>.
    /// </summary>
    /// <param name="streamIn">An input stream to read properties from.</param>
    /// <exception cref="Exception">If the stream source is invalid.</exception>
    public void Load(Stream streamIn)
    {
        new PropertyFileReader(this).Parse(streamIn);
    }

    /// <summary>
    /// Load Java Properties from a stream with the specified encoding and
    /// expecting the format as described in <see cref="PropertyFileReader"/>.
    /// </summary>
    /// <param name="streamIn">An input stream to read properties from.</param>
    /// <param name="encoding">The stream's encoding.</param>
    public void Load(Stream streamIn, Encoding encoding)
    {
        new PropertyFileReader(this).Parse(streamIn, encoding);
    }

    /// <summary>
    /// Store the contents of this collection of properties to the stream in the format
    /// used for Java ".properties" files using an instance of <see cref="PropertyFileWriter"/>.
    /// The keys and values will be minimally escaped to ensure special characters are read back
    /// in properly.  Keys are not sorted.  The file will begin with a comment identifying the
    /// date - and an additional comment may be included.
    /// </summary>
    /// <param name="streamOut">An output stream to write the properties to.</param>
    /// <param name="comments">Optional additional comment to include at the head of the output.</param>
    public void Store(Stream streamOut, string comments)
    {
        new PropertyFileWriter(this).Write(streamOut, comments);
    }

    /// <summary>
    /// Get the value for the specified key value.  If the key is not found, then return the
    /// default value - and if still not found, return null.
    /// </summary>
    /// <param name="key">The key whose value should be returned.</param>
    /// <returns>The value corresponding to the key - or null if not found.</returns>
    public string GetProperty(string key)
    {
        object objectValue = this[key];

        if (objectValue != null)
        {
            return AsString(objectValue);
        }

        return defaults != null ? AsString(defaults[key]) : null;
    }

    /// <summary>
    /// Get the value for the specified key value.  If the key is not found, then return the
    /// default value - and if still not found, return <c>defaultValue</c>.
    /// </summary>
    /// <param name="key">The key whose value should be returned.</param>
    /// <param name="defaultValue">The default value if the key is not found.</param>
    /// <returns>The value corresponding to the key - or null if not found.</returns>
    public string GetProperty(string key, string defaultValue)
    {
        return GetProperty(key) ?? defaultValue;
    }

    public bool GetProperty(string key, bool defaultValue)
    {
        try
        {
            return Convert.ToBoolean(GetProperty(key));
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    public int GetProperty(string key, int defaultValue)
    {
        try
        {
            return Convert.ToInt32(GetProperty(key));
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    public float GetProperty(string key, float defaultValue)
    {
        try
        {
            return Convert.ToSingle(GetProperty(key));
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Set the value for a property key.  The old value is returned - if any.
    /// </summary>
    /// <param name="key">The key whose value is to be set.</param>
    /// <param name="newValue">The new value off the key.</param>
    /// <returns>The original value of the key - as a string.</returns>
    public string SetProperty(string key, string newValue)
    {
        string oldValue = AsString(this[key]);
        this[key] = newValue;

        return oldValue;
    }

    public string SetProperty(string key, bool newValue)
    {
        return SetProperty(key, Convert.ToString(newValue));
    }

    public string SetProperty(string key, int newValue)
    {
        return SetProperty(key, Convert.ToString(newValue));
    }

    public string SetProperty(string key, float newValue)
    {
        return SetProperty(key, Convert.ToString(newValue, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Returns a list of all the properties available in this instance - including the
    /// defaults.
    /// </summary>
    /// <returns>An enumarator for all of the keys including defaults.</returns>
    public List<string> PropertyNames()
    {
        List<string> keys = [];

        if (defaults != null)
        {
            for (IEnumerator e = Keys.GetEnumerator(); e.MoveNext();)
            {
                keys.Add(AsString(e.Current));
            }
        }

        return keys;
    }

    /// <summary>
    /// A utility method to safely convert an <c>Object</c> to a <c>string</c>.
    /// </summary>
    /// <param name="o">An Object or null to be returned as a string.</param>
    /// <returns>string value of the object - or null.</returns>
    private string AsString(object o)
    {
        return o?.ToString();
    }
}
