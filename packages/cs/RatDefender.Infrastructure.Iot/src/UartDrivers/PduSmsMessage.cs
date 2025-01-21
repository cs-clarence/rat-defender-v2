using System.Globalization;
using System.Text;

namespace RatDefender.Infrastructure.Iot.UartDrivers;

internal class PduSmsMessage
{
    /// <summary>
    /// The maximum length of a concatenated SMS message.
    /// </summary>
    public const int MaximumConcatenatedSmsTextLength = 153;

    /// <summary>
    /// The maximum length of a SMS message.
    /// </summary>
    public const int MaximumSmsTextLength = 160;

    /// <summary>
    /// The default data coding scheme: GSM-7.
    /// </summary>
    private const byte DefaultDataCodingScheme = 0;

    /// <summary>
    /// The default message reference: Automatic.
    /// </summary>
    private const byte DefaultMessageReference = 0;

    /// <summary>
    /// The default PDU header: SMS-SUBMIT.
    /// </summary>
    private const byte DefaultPduHeader = 0x01;

    /// <summary>
    /// PDU header: SMS-SUBMIT with user header.
    /// </summary>
    private const byte DefaultPduHeaderWithUserHeader = 0x41;

    /// <summary>
    /// The default protocol identifier: No higher level protocol.
    /// </summary>
    private const byte DefaultProtocolIdentifier = 0;

    /// <summary>
    /// The default SMSC (Short Message Service Center) number length: Use the SMSC number stored in the phone.
    /// </summary>
    private const byte DefaultSmsc = 0;

    /// <summary>
    /// The GSM-7 charset.
    /// </summary>
    private const string Gsm7CharSet =
        "@£$¥èéùìòÇ\nØø\rÅåΔ_ΦΓΛΩΠΨΣΘΞ\x1BÆæßÉ !\"#¤%&'()*+,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà";

    /// <summary>
    /// The concatenated SMS message part number.
    /// </summary>
    private byte _concatenatedMessagePartNumber;

    /// <summary>
    /// The concatenated SMS message reference number.
    /// </summary>
    private byte _concatenatedMessageReferenceNumber;

    /// <summary>
    /// The total of concatenated SMS message parts.
    /// </summary>
    private byte _concatenatedMessageTotalParts;

    /// <summary>
    /// The encoded number.
    /// </summary>
    private string _encodedNumber;

    /// <summary>
    /// The encoded text.
    /// </summary>
    private string _encodedText;

    /// <summary>
    /// The number with country code.
    /// </summary>
    private long _number;

    /// <summary>
    /// The text.
    /// </summary>
    private string _text;

    /// <summary>
    /// Initializes a new instance of the <see cref="PduSmsMessage" /> class.
    /// </summary>
    /// <param name="number">The number with country code.</param>
    /// <param name="text">The text.</param>
    public PduSmsMessage(long number, string text)
    {
        // Set the defaults.
        DataCodingScheme = DefaultDataCodingScheme;
        MessageReference = DefaultMessageReference;
        PduHeader = DefaultPduHeader;
        ProtocolIdentifier = DefaultProtocolIdentifier;
        Smsc = DefaultSmsc;

        Number = number;
        Text = text;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PduSmsMessage" /> class.
    /// </summary>
    /// <param name="number">The number with country code.</param>
    /// <param name="text">The text.</param>
    /// <param name="concatenatedMessageReferenceNumber">The concatenated SMS message reference number.</param>
    /// <param name="concatenatedMessagePartNumber">The concatenated SMS message part number.</param>
    /// <param name="concatenatedMessageTotalParts">The total of concatenated SMS message parts.</param>
    public PduSmsMessage(
        long number,
        string text,
        byte concatenatedMessageReferenceNumber,
        byte concatenatedMessagePartNumber,
        byte concatenatedMessageTotalParts)
    {
        // Set the defaults.
        DataCodingScheme = DefaultDataCodingScheme;
        PduHeader = DefaultPduHeaderWithUserHeader;
        ProtocolIdentifier = DefaultProtocolIdentifier;
        Smsc = DefaultSmsc;

        Number = number;
        Text = text;
        ConcatenatedMessageReferenceNumber =
            concatenatedMessageReferenceNumber;
        ConcatenatedMessagePartNumber = concatenatedMessagePartNumber;
        ConcatenatedMessageTotalParts = concatenatedMessageTotalParts;

        // The message reference must be different on each concatenated message.
        MessageReference = (byte)(DefaultMessageReference +
            ConcatenatedMessagePartNumber - 1);
    }

    /// <summary>
    /// Gets the concatenated SMS message part number.
    /// </summary>
    public byte ConcatenatedMessagePartNumber
    {
        get => _concatenatedMessagePartNumber;

        private set
        {
            if (PduHeader == DefaultPduHeader)
            {
                throw new ArgumentException(
                    "Cannot set value if message is not concatenated type.",
                    "value");
            }

            _concatenatedMessagePartNumber = value;
        }
    }

    /// <summary>
    /// Gets the concatenated SMS message reference number.
    /// </summary>
    public byte ConcatenatedMessageReferenceNumber
    {
        get => _concatenatedMessageReferenceNumber;

        private set
        {
            if (PduHeader == DefaultPduHeader)
            {
                throw new ArgumentException(
                    "Cannot set value if message is not concatenated type.",
                    "value");
            }

            _concatenatedMessageReferenceNumber = value;
        }
    }

    /// <summary>
    /// Gets the total of concatenated SMS message parts.
    /// </summary>
    public byte ConcatenatedMessageTotalParts
    {
        get => _concatenatedMessageTotalParts;

        private set
        {
            if (PduHeader == DefaultPduHeader)
            {
                throw new ArgumentException(
                    "Cannot set value if message is not concatenated type.",
                    "value");
            }

            _concatenatedMessageTotalParts = value;
        }
    }

    /// <summary>
    /// Gets the data coding scheme.
    /// </summary>
    public byte DataCodingScheme { get; private set; }

    /// <summary>
    /// Gets the length of the PDU formatted SMS in bytes.
    /// </summary>
    public int Length =>
        // The length must not take into account the length of the SMSC field.
        (ToString().Length - 1) / 2;

    /// <summary>
    /// Gets the message reference number.
    /// </summary>
    public byte MessageReference { get; private set; }

    /// <summary>
    /// Gets the number with country code.
    /// </summary>
    public long Number
    {
        get => _number;

        private set
        {
            _encodedNumber = PduSmsMessage.EncodeNumber(value);
            _number = value;
        }
    }

    /// <summary>
    /// Gets the PDU formatted SMS message header.
    /// </summary>
    public byte PduHeader { get; private set; }

    /// <summary>
    /// Gets the protocol identifier.
    /// </summary>
    public byte ProtocolIdentifier { get; private set; }

    /// <summary>
    /// Gets the SMSC (Short Message Service Center) number length.
    /// </summary>
    public byte Smsc { get; private set; }

    /// <summary>
    /// Gets the text.
    /// </summary>
    public string Text
    {
        get => _text;

        private set
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            switch (PduHeader)
            {
                case DefaultPduHeader:
                    if (value.Length > MaximumSmsTextLength)
                    {
                        throw new ArgumentException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Text length cannot be greater than {0}.",
                                MaximumSmsTextLength), "value");
                    }

                    _encodedText = EncodeText(value);
                    break;

                case DefaultPduHeaderWithUserHeader:
                    if (value.Length >
                        MaximumConcatenatedSmsTextLength)
                    {
                        throw new ArgumentException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Text length cannot be greater than {0}.",
                                MaximumConcatenatedSmsTextLength),
                            "value");
                    }

                    // The User header in a concatenated PDU formatted SMS message is 48 bits long, since were using septets for the text we must align the text to the next multiple of 7 by adding a 1-bit padding.
                    _encodedText =
                        EncodeTextWith1BitPadding(value);
                    break;

                default:
                    throw new InvalidOperationException(
                        "Invalid PDU header detected.");
            }

            _text = value;
        }
    }

    /// <summary>
    /// Gets a series of concatenated PDU formatted SMS messages.
    /// </summary>
    /// <param name="number">The number with country code.</param>
    /// <param name="text">The text.</param>
    public static IEnumerable<PduSmsMessage> GetConcatenatedMessages(
        long number, string text)
    {
        if (text == null)
        {
            throw new ArgumentNullException("text");
        }

        if (text.Length <= PduSmsMessage.MaximumSmsTextLength)
        {
            throw new ArgumentException(
                string.Format(CultureInfo.InvariantCulture,
                    "The text length must be longer than {0}.",
                    PduSmsMessage.MaximumSmsTextLength), "text");
        }

        if (text.Length / PduSmsMessage.MaximumConcatenatedSmsTextLength > 255)
        {
            throw new ArgumentException("The text length is too big.", "text");
        }

        // The reference part number is an arbitrary number but must be equal for all the concatenated SMS messages.
        var concatenatedMessageReferenceNumber =
            (byte)new Random().Next(0, 255);

        var concatenatedMessagePartNumber = (byte)0;
        var concatenatedMessageTotalParts = (byte)Math.Ceiling(
            ((double)text.Length) /
            MaximumConcatenatedSmsTextLength);

        for (var i = 0;
             i < text.Length;
             i += PduSmsMessage.MaximumConcatenatedSmsTextLength)
        {
            ++concatenatedMessagePartNumber;

            var textPart =
                i + PduSmsMessage.MaximumConcatenatedSmsTextLength < text.Length
                    ? text.Substring(i,
                        PduSmsMessage.MaximumConcatenatedSmsTextLength)
                    : text.Substring(i, text.Length - i);

            yield return new PduSmsMessage(
                number,
                textPart,
                concatenatedMessageReferenceNumber,
                concatenatedMessagePartNumber,
                concatenatedMessageTotalParts);
        }
    }

    /// <summary>
    /// Returns a string representing the PDU formatted SMS message in hexadecimal.
    /// </summary>
    public override string ToString()
    {
        if (this.PduHeader == PduSmsMessage.DefaultPduHeader)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:X2}{1:X2}{2:X2}{3}{4:X2}{5:X2}{6:X2}{7}",
                this.Smsc,
                this.PduHeader,
                this.MessageReference,
                this._encodedNumber,
                this.ProtocolIdentifier,
                this.DataCodingScheme,
                this._text.Length, // Size of the user data in septets.
                this._encodedText);
        }
        else if (this.PduHeader == PduSmsMessage.DefaultPduHeaderWithUserHeader)
        {
            /*
             * User header format:
             *
             * 05      : UDHL (User Data Header Length).
             * 00      : IEI (Information Element Identifier) for concatenated short messages.
             * 03      : IEIL (Information Element Data Length).
             * {9:X2}  : Reference number.
             * {10:X2} : Total parts.
             * {11:X2} : Part number.
             */

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:X2}{1:X2}{2:X2}{3}{4:X2}{5:X2}{6:X2}{7:X6}{8:X2}{9:X2}{10:X2}{11}",
                Smsc,
                PduHeader,
                MessageReference,
                _encodedNumber,
                ProtocolIdentifier,
                DataCodingScheme,
                7 + _text
                    .Length, // Size of the user data in septets (7 is the size of the user header).
                0x050003, // User header starts.
                _concatenatedMessageReferenceNumber,
                _concatenatedMessageTotalParts,
                _concatenatedMessagePartNumber, // User header ends.
                _encodedText);
        }

        throw new InvalidOperationException("Invalid PDU header detected.");
    }

    /// <summary>
    /// Encodes a number for a PDU formatted SMS message.
    /// </summary>
    /// <param name="number">The number with country code.</param>
    private static string EncodeNumber(long number)
    {
        var reverseNibbleEncodingNumber =
            new StringBuilder(number.ToString(CultureInfo.InvariantCulture));
        var numberOfDigits = reverseNibbleEncodingNumber.Length;

        // The reverse nibble encoding is applied.
        if (numberOfDigits % 2 != 0)
        {
            reverseNibbleEncodingNumber.Append("F");
        }

        for (var i = 0; i < reverseNibbleEncodingNumber.Length; i += 2)
        {
            var tmp = reverseNibbleEncodingNumber[i];

            reverseNibbleEncodingNumber[i] = reverseNibbleEncodingNumber[i + 1];
            reverseNibbleEncodingNumber[i + 1] = tmp;
        }

        /*
         * Format:
         *
         * {0:X2} : Number of digits.
         * 91     : International number indicator.
         * {1}    : Reverse nibble encoded number optionally ending with F.
         */
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0:X2}91{1}",
            numberOfDigits,
            reverseNibbleEncodingNumber);
    }

    /// <summary>
    /// Encodes a text to GSM-7 for a PDU formatted SMS message.
    /// </summary>
    /// <param name="text">The text.</param>
    private static string EncodeText(string text)
    {
        if (text == null)
        {
            throw new ArgumentNullException("text");
        }

        // Gets the GSM-7 charset bytes.
        var bytes = new List<byte>(text.Length);

        foreach (char c in text)
        {
            var index = PduSmsMessage.Gsm7CharSet.IndexOf(c);

            if (index != -1)
            {
                bytes.Add((byte)index);
            }
            else
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture,
                        "An invalid character was found: '{0}'.", c), "text");
            }
        }

        // Encodes the GSM-7 bytes for PDU.
        var offset = 0;
        for (var i = 0; i < bytes.Count; ++i)
        {
            // Increments a counter with range 0 to 7.
            offset = ++offset % 8;

            // If there's a next byte.
            if (i < bytes.Count - 1)
            {
                if (offset != 0)
                {
                    // Remove bits of the current byte that were included in the previous byte.
                    bytes[i] >>= offset - 1;

                    // Insert bits of the next byte into the current byte.
                    bytes[i] |= (byte)(bytes[i + 1] << (8 - offset));
                }
                else
                {
                    // Remove current byte because all its bits where inserted on the previous byte.
                    bytes.RemoveAt(i--);
                }
            }
            else
            {
                if (offset != 0)
                {
                    // Remove bits of the current byte that were included in the previous byte.
                    bytes[i] >>= offset - 1;
                }
                else
                {
                    // Remove current byte because all its bits where inserted on the previous byte.
                    bytes.RemoveAt(i);
                }
            }
        }

        // Print bytes in hex format.
        var result = new StringBuilder(bytes.Count * 2);

        foreach (var b in bytes)
        {
            result.AppendFormat("{0:X2}", b);
        }

        return result.ToString();
    }

    /// <summary>
    /// Encodes a text to GSM-7 for a concatenated PDU formatted SMS messages.
    /// </summary>
    /// <param name="text">The text.</param>
    private static string EncodeTextWith1BitPadding(string text)
    {
        if (text == null)
        {
            throw new ArgumentNullException("text");
        }

        // Gets the GSM-7 charset bytes.
        var bytes = new List<byte>(text.Length);

        foreach (char c in text)
        {
            var index = PduSmsMessage.Gsm7CharSet.IndexOf(c);

            if (index != -1)
            {
                bytes.Add((byte)index);
            }
            else
            {
                throw new ArgumentException(
                    "An invalid character was found: '{0}'.", "text");
            }
        }

        // Insert 1 bit of padding.
        if (bytes.Count > 0)
        {
            bytes[0] <<= 1;
        }

        // Encodes the GSM-7 bytes for PDU.
        var offset = 0;
        for (var i = 1; i < bytes.Count; ++i)
        {
            // Increments a counter with range 0 to 7.
            offset = ++offset % 8;

            // If there's a next byte.
            if (i < bytes.Count - 1)
            {
                if (offset != 0)
                {
                    // Remove bits of the current byte that were included in the previous byte.
                    bytes[i] >>= offset - 1;

                    // Insert bits of the next byte into the current byte.
                    bytes[i] |= (byte)(bytes[i + 1] << (8 - offset));
                }
                else
                {
                    // Remove current byte because all its bits where inserted on the previous byte.
                    bytes.RemoveAt(i--);
                }
            }
            else
            {
                if (offset != 0)
                {
                    // Remove bits of the current byte that were included in the previous byte.
                    bytes[i] >>= offset - 1;
                }
                else
                {
                    // Remove current byte because all its bits where inserted on the previous byte.
                    bytes.RemoveAt(i);
                }
            }
        }

        // Print bytes in hex format.
        var result = new StringBuilder(bytes.Count * 2);

        foreach (var b in bytes)
        {
            result.AppendFormat("{0:X2}", b);
        }

        return result.ToString();
    }
}
