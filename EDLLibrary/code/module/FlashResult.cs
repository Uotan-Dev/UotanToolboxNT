namespace EDLLibrary.code.module;

public class FlashResult
{
    private bool _result;

    private string _msg;

    public bool Result
    {
        get
        {
            return _result;
        }
        set
        {
            _result = value;
        }
    }

    public string Msg
    {
        get
        {
            return _msg;
        }
        set
        {
            _msg = value;
        }
    }
}
