namespace WuXingLibrary.code.module;

public class CommandPacket
{
    private int _command;

    private int _Lengthgth;

    private int _Versionnumber;

    private int _Versioncompatible;

    private int _commandpacketLengthgth;

    private int _Mode;

    public int Command
    {
        get
        {
            return _command;
        }
        set
        {
            _command = value;
        }
    }

    public int Length
    {
        get
        {
            return _Lengthgth;
        }
        set
        {
            _Lengthgth = value;
        }
    }

    public int VersionNumber
    {
        get
        {
            return _Versionnumber;
        }
        set
        {
            _Versionnumber = value;
        }
    }

    public int VersionCompatible
    {
        get
        {
            return _Versioncompatible;
        }
        set
        {
            _Versioncompatible = value;
        }
    }

    public int CommandPacketLengthgth
    {
        get
        {
            return _commandpacketLengthgth;
        }
        set
        {
            _commandpacketLengthgth = value;
        }
    }

    public int Mode
    {
        get
        {
            return _Mode;
        }
        set
        {
            _Mode = value;
        }
    }

    public CommandPacket()
    {
    }

    public CommandPacket(byte[] arr)
    {
        if (arr.Length < 48)
        {
            return;
        }
        for (int i = 0; i < arr.Length; i += 4)
        {
            switch (i)
            {
                case 0:
                    _command = arr[i];
                    break;
                case 4:
                    _Lengthgth = arr[i];
                    break;
                case 8:
                    _Versionnumber = arr[i];
                    break;
                case 12:
                    _Versioncompatible = arr[i];
                    break;
                case 16:
                    _commandpacketLengthgth = arr[i];
                    break;
                case 20:
                    _Mode = arr[i];
                    break;
            }
        }
    }
}
