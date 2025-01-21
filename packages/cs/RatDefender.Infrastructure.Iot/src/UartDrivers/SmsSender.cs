using System.Globalization;
using System.IO.Ports;
using Microsoft.Extensions.Logging;

namespace RatDefender.Infrastructure.Iot.UartDrivers;

internal class SmsSender : IDisposable
{
    /// <summary>
    /// The baud rate of the serial port.
    /// </summary>
    private const int BaudRate = 115200;

    /// <summary>
    /// The initial sleep time after opening the serial port.
    /// </summary>
    private const int InitialSleepTime = 500;

    /// <summary>
    /// The PDU formatted SMS messages to send.
    /// </summary>
    private readonly IEnumerable<PduSmsMessage> _pduMessages;

    /// <summary>
    /// The serial port.
    /// </summary>
    private SerialPort _port;

    private ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmsSender" /> class.
    /// </summary>
    /// <param name="portName">The name of the serial port.</param>
    /// <param name="countryCode">The number's country code.</param>
    /// <param name="localNumber">The number in local format.</param>
    /// <param name="text">The text to send.</param>
    /// <param name="logger">The logger if any.</param>
    public SmsSender(string portName, short countryCode, long localNumber,
        string text, ILogger? logger = null)
    {
        _logger = logger;
        if (string.IsNullOrWhiteSpace(portName))
        {
            throw new ArgumentException("A port name must be specified.",
                nameof(portName));
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentNullException(nameof(text),
                "A text must be specified.");
        }

        // Port creation.
        _port = new SerialPort(portName, BaudRate);
        _port.DataReceived += DataReceived;

        // Number composition.
        var numberOfDigits =
            localNumber.ToString(CultureInfo.InvariantCulture).Length;
        var fullNumber = ((long)(countryCode * Math.Pow(10, numberOfDigits))) +
                         localNumber;

        _pduMessages = text.Length <= PduSmsMessage.MaximumSmsTextLength
            ? [new PduSmsMessage(fullNumber, text)]
            : PduSmsMessage.GetConcatenatedMessages(fullNumber, text);
    }

    /// <summary>
    /// Gets or sets a value indicating whether debug mode is activated.
    /// </summary>
    public static bool Debug { get; set; }

    /// <summary>
    /// Closes the serial port.
    /// </summary>
    public void Dispose()
    {
        _port.Dispose();
    }

    /// <summary>
    /// Sends the SMS message.
    /// </summary>
    public void Send()
    {
        lock (_port)
        {
            OpenPort();
            SetPduMode();

            foreach (var pduMessage in _pduMessages)
            {
                SetSize(pduMessage);
                SetContent(pduMessage);
            }
        }
    }

    /// <summary>
    /// Notifies waiting threads that there is data available.
    /// </summary>
    private void DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        lock (_port)
        {
            Monitor.Pulse(_port);
        }
    }

    /// <summary>
    /// Blocking call that returns a non-null/non-empty response from the serial port.
    /// </summary>
    private string GetPortResponse()
    {
        string result;

        do
        {
            Monitor.Wait(_port);
        } while (string.IsNullOrWhiteSpace(result = this._port.ReadExisting()));

        return result;
    }

    /// <summary>
    /// Opens the serial port.
    /// </summary>
    private void OpenPort()
    {
        _port.Open();
        Thread.Sleep(SmsSender.InitialSleepTime);
    }

    /// <summary>
    /// Sets the PDU formatted SMS message content.
    /// </summary>
    /// <param name="pduMessage">The PDU formatted SMS message to send.</param>
    private void SetContent(PduSmsMessage pduMessage)
    {
        // Set the content.
        _port.Write(string.Format(CultureInfo.InvariantCulture, "{0}\x1A",
            pduMessage));

        // Validate response.
        while (true)
        {
            var response = GetPortResponse();

            if (response.IndexOf("ERROR", StringComparison.OrdinalIgnoreCase) !=
                -1)
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.InvariantCulture,
                        "Got error response '{0}'.", response));
            }

            if (Debug)
            {
                _logger?.LogDebug("SetContent() got response: '{0}'.",
                    response.Trim());
            }

            if (response.IndexOf("+CMGS:",
                    StringComparison.OrdinalIgnoreCase) != -1)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Sets the mode to PDU.
    /// </summary>
    private void SetPduMode()
    {
        // Set the mode.
        _port.Write("AT+CMGF=0\r\n");

        // Validate response.
        while (true)
        {
            var response = GetPortResponse();

            if (response.IndexOf("ERROR", StringComparison.OrdinalIgnoreCase) !=
                -1)
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.InvariantCulture,
                        "Got error response '{0}'.", response));
            }

            if (Debug)
            {
                _logger?.LogDebug("SetContent() got response: '{0}'.",
                    response.Trim());
            }

            if (response.IndexOf("OK", StringComparison.OrdinalIgnoreCase) !=
                -1)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Sets the size of the PDU.
    /// </summary>
    /// <param name="pduMessage">The PDU formatted SMS message to send.</param>
    private void SetSize(PduSmsMessage pduMessage)
    {
        // Set the size.
        _port.Write(string.Format(CultureInfo.InvariantCulture,
            "AT+CMGS={0}\r\n", pduMessage.Length));

        // Validate response.
        while (true)
        {
            var response = GetPortResponse();

            if (response.IndexOf("ERROR", StringComparison.OrdinalIgnoreCase) !=
                -1)
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.InvariantCulture,
                        "Got error response '{0}'.", response));
            }

            if (Debug)
            {
                _logger?.LogDebug("SetContent() got response: '{0}'.",
                    response.Trim());
            }

            if (response.IndexOf(">", StringComparison.OrdinalIgnoreCase) != -1)
            {
                break;
            }
        }
    }
}