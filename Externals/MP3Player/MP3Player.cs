/* 
 * 
 * MP3Player class provides basic functionallity for playing mp3 files.
 * Aside from the various methods it implemants events which notify their subscribers for opening files, pausing, etc.
 * This is done for boosting performance on your applications using this class, because instead of checking for info
 * on the player status over a certain time period and loosing performance, you can subscribe for an event and handle it when it fires.
 * This class also doesn't throw exceptions. The error handling is done by an event, because the probable errors which may occur are not
 * severe and the application just needs to be notified for these failures on the fly...
 * Share your source and modify this code to your heart's content, just don't change this section.
 * If you have questions, suggestions or just need to make your oppinion heard my email is krazymir@gmail.com
 * Krasimir kRAZY Kalinov 2006
 * 
 * PS: This source will only work on MS Windows, since it uses the MCI(Media Control Interface) integrated into this OS.
 * Sorry .Gnu and Mono fans! I hope soon to have enough time to get busy working on similar class for these engines...
 * 
 */

using System;
using System.Text;
using System.Runtime.InteropServices;

#region Event Argumenst for the events implemented by the wrapper class
public class OpenFileEventArgs : EventArgs
{
    public OpenFileEventArgs(string filename)
    {
        this.FileName = filename;
    }
    public readonly string FileName;
}

public class PlayFileEventArgs : EventArgs
{
    public PlayFileEventArgs()
    {
    }
}

public class PauseFileEventArgs : EventArgs
{
    public PauseFileEventArgs()
    {
    }
}

public class StopFileEventArgs : EventArgs
{
    public StopFileEventArgs()
    {
    }
}

public class CloseFileEventArgs : EventArgs
{
    public CloseFileEventArgs()
    {
    }
}

public class ErrorEventArgs : EventArgs
{
    public ErrorEventArgs(long Err)
    {
        this.ErrNum = Err;
    }

    public readonly long ErrNum;
}
#endregion

public class MP3Player
{
    private string m_command_str;
    private string m_filename;
    private bool m_is_opened;
    private bool m_is_playing;
    private bool m_is_paused;
    private bool m_is_looping;
    private bool m_is_muted_all;
    private bool m_is_muted_left;
    private bool m_is_muted_right;
    private int m_volume_right;
    private int m_volume_left;
    private int m_volume_all;
    private int m_volume_treble;
    private int m_volume_bass;
    private int m_volume_balance;
    private ulong m_length;
    private long m_error;

    [DllImport("winmm.dll")]
    private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);

    public MP3Player()
    {
        m_is_opened = false;
        m_command_str = "";
        m_filename = "";
        m_is_playing = false;
        m_is_paused = false;
        m_is_looping = false;
        m_is_muted_all = false;
        m_is_muted_left = false;
        m_is_muted_right = false;
        m_volume_right = 1000;
        m_volume_left = 1000;
        m_volume_all = 1000;
        m_volume_treble = 1000;
        m_volume_bass = 1000;
        m_length = 0;
        m_volume_balance = 0;
        m_error = 0;
    }

    #region Volume
    public bool MuteAll
    {
        get
        {
            return m_is_muted_all;
        }
        set
        {
            m_is_muted_all = value;
            if (m_is_muted_all)
            {
                m_command_str = "setaudio MediaFile off";
                if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
            }
            else
            {
                m_command_str = "setaudio MediaFile on";
                if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
            }
        }

    }

    public bool MuteLeft
    {
        get
        {
            return m_is_muted_left;
        }
        set
        {
            m_is_muted_left = value;
            if (m_is_muted_left)
            {
                m_command_str = "setaudio MediaFile left off";
                if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
            }
            else
            {
                m_command_str = "setaudio MediaFile left on";
                if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
            }
        }

    }

    public bool MuteRight
    {
        get
        {
            return m_is_muted_right;
        }
        set
        {
            m_is_muted_right = value;
            if (m_is_muted_right)
            {
                m_command_str = "setaudio MediaFile right off";
                if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
            }
            else
            {
                m_command_str = "setaudio MediaFile right on";
                if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
            }
        }

    }

    public int VolumeAll
    {
        get
        {
            return m_volume_all;
        }
        set
        {
            if (m_is_opened && (value >= 0 && value <= 1000))
            {
                m_volume_all = value;
                m_command_str = String.Format("setaudio MediaFile volume to {0}", m_volume_all);
                if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
            }
        }
    }

    public int VolumeLeft
    {
        get
        {
            return m_volume_left;
        }
        set
        {
            if (m_is_opened && (value >= 0 && value <= 1000))
            {
                m_volume_left = value;
                m_command_str = String.Format("setaudio MediaFile left volume to {0}", m_volume_left);
                if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
            }
        }
    }

    public int VolumeRight
    {
        get
        {
            return m_volume_right;
        }
        set
        {
            if (m_is_opened && (value >= 0 && value <= 1000))
            {
                m_volume_right = value;
                m_command_str = String.Format("setaudio MediaFile right volume to {0}", m_volume_right);
                if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
            }
        }
    }

    public int VolumeTreble
    {
        get
        {
            return m_volume_treble;
        }
        set
        {
            if (m_is_opened && (value >= 0 && value <= 1000))
            {
                m_volume_treble = value;
                m_command_str = String.Format("setaudio MediaFile treble to {0}", m_volume_treble);
                if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
            }
        }
    }

    public int VolumeBass
    {
        get
        {
            return m_volume_bass;
        }
        set
        {
            if (m_is_opened && (value >= 0 && value <= 1000))
            {
                m_volume_bass = value;
                m_command_str = String.Format("setaudio MediaFile bass to {0}", m_volume_bass);
                if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
            }
        }
    }

    public int Balance
    {
        get
        {
            return m_volume_balance;
        }
        set
        {
            if (m_is_opened && (value >= -1000 && value <= 1000))
            {
                m_volume_balance = value;
                if (value < 0)
                {
                    m_command_str = "setaudio MediaFile left volume to 1000";
                    if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
                    m_command_str = String.Format("setaudio MediaFile right volume to {0}", 1000 + value);
                    if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
                }
                else
                {
                    m_command_str = "setaudio MediaFile right volume to 1000";
                    if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
                    m_command_str = String.Format("setaudio MediaFile left volume to {0}", 1000 - value);
                    if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
                }
            }
        }
    }
    #endregion

    #region Main Functions

    public string FileName
    {
        get
        {
            return m_filename;
        }
    }

    public bool Opened
    {
        get
        {
            return m_is_opened;
        }
    }
    public bool Playing
    {
        get
        {
            return m_is_playing;
        }
    }
    public bool Paused
    {
        get
        {
            return m_is_paused;
        }
    }
    public bool Stopped
    {
        get
        {
            return !m_is_playing;
        }
    }
    public bool Closed
    {
        get
        {
            return !m_is_opened;
        }
    }
    public bool Looping
    {
        get
        {
            return m_is_looping;
        }
        set
        {
            m_is_looping = value;
        }
    }


    public void Seek(ulong millisecs)
    {
        if (m_is_opened && millisecs <= m_length)
        {
            if (m_is_playing)
            {
                if (m_is_paused)
                {
                    m_command_str = String.Format("seek MediaFile to {0}", millisecs);
                    if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
                }
                else
                {
                    m_command_str = String.Format("seek MediaFile to {0}", millisecs);
                    if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
                    m_command_str = "play MediaFile";
                    if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
                }
            }
        }
    }

    private void CalculateLength()
    {
        StringBuilder str = new StringBuilder(128);
        mciSendString("status MediaFile length", str, 128, IntPtr.Zero);
        if (str.Length > 0)
        {
            m_length = Convert.ToUInt64(str.ToString());
        }
        else
        {
            m_length = 0;
        }
    }

    public ulong Length
    {
        get
        {
            if (m_is_opened) return m_length;
            else return 0;
        }
    }

    public void Open(string filename)
    {
        if (!m_is_opened)
        {
            m_command_str = "open \"" + filename + "\" type mpegvideo alias MediaFile";
            if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
            m_filename = filename;
            m_is_opened = true;
            m_is_playing = false;
            m_is_paused = false;
            m_command_str = "set MediaFile time format milliseconds";
            if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
            m_command_str = "set MediaFile seek exactly on";
            if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
            CalculateLength();
            if (m_length > 0)
            {
                OnOpenFile(new OpenFileEventArgs(filename));
            }
            else
            {
                throw new Exception("Audio file length is zero!");
            }
        }
        else
        {
            this.Close();
            this.Open(filename);
        }
    }

    public void Play()
    {
        if (m_is_opened)
        {
            if (!m_is_playing)
            {
                m_is_playing = true;
                m_command_str = "play MediaFile";
                if (m_is_looping) m_command_str += " REPEAT";
                if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
                OnPlayFile(new PlayFileEventArgs());
            }
            else
            {
                if (!m_is_paused)
                {
                    m_command_str = "seek MediaFile to start";
                    if (m_is_looping) m_command_str += " REPEAT";
                    if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
                    m_command_str = "play MediaFile";
                    if (m_is_looping) m_command_str += " REPEAT";
                    if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
                    OnPlayFile(new PlayFileEventArgs());
                }
                else
                {
                    m_is_paused = false;
                    m_command_str = "play MediaFile";
                    if (m_is_looping) m_command_str += " REPEAT";
                    if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
                    OnPlayFile(new PlayFileEventArgs());
                }
            }
        }
    }

    public void Pause()
    {
        if (m_is_opened)
        {
            if (!m_is_paused)
            {
                m_is_paused = true;
                m_command_str = "pause MediaFile";
                if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
                OnPauseFile(new PauseFileEventArgs());
            }
            else
            {
                m_is_paused = false;
                m_command_str = "play MediaFile";
                if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
                OnPlayFile(new PlayFileEventArgs());
            }
        }
    }

    public void Stop()
    {
        try
        {
            if (m_is_opened && m_is_playing)
            {
                m_is_playing = false;
                m_is_paused = false;
                m_command_str = "seek MediaFile to start";
                if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
                m_command_str = "stop MediaFile";
                if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
                OnStopFile(new StopFileEventArgs());
            }
        }
        catch
        {
            m_is_playing = true;
            m_is_paused = true;
        }
    }

    public ulong Position
    {
        get
        {
            try
            {
                if (m_is_opened && m_is_playing)
                {
                    StringBuilder str = new StringBuilder(128);
                    m_command_str = "status MediaFile position";
                    if ((m_error = mciSendString(m_command_str, str, 128, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
                    if (str.Length > 0)
                    {
                        return Convert.ToUInt64(str.ToString());
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }
    }

    public void Close()
    {
        if (m_is_opened)
        {
            m_command_str = "close MediaFile";
            if ((m_error = mciSendString(m_command_str, null, 0, IntPtr.Zero)) != 0) OnError(new ErrorEventArgs(m_error));
            m_is_opened = false;
            m_is_playing = false;
            m_is_paused = false;
            OnCloseFile(new CloseFileEventArgs());
        }
    }

    #endregion

    #region Event Handling

    public delegate void OpenFileEventHandler(object sender, OpenFileEventArgs e);
    public delegate void PlayFileEventHandler(object sender, PlayFileEventArgs e);
    public delegate void PauseFileEventHandler(object sender, PauseFileEventArgs e);
    public delegate void StopFileEventHandler(object sender, StopFileEventArgs e);
    public delegate void CloseFileEventHandler(object sender, CloseFileEventArgs e);
    public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);

    public event OpenFileEventHandler OpenFile;
    public event PlayFileEventHandler PlayFile;
    public event PauseFileEventHandler PauseFile;
    public event StopFileEventHandler StopFile;
    public event CloseFileEventHandler CloseFile;
    public event ErrorEventHandler Error;

    protected virtual void OnOpenFile(OpenFileEventArgs e)
    {
        if (OpenFile != null) OpenFile(this, e);
    }
    protected virtual void OnPlayFile(PlayFileEventArgs e)
    {
        if (PlayFile != null) PlayFile(this, e);
    }
    protected virtual void OnPauseFile(PauseFileEventArgs e)
    {
        if (PauseFile != null) PauseFile(this, e);
    }
    protected virtual void OnStopFile(StopFileEventArgs e)
    {
        if (StopFile != null) StopFile(this, e);
    }
    protected virtual void OnCloseFile(CloseFileEventArgs e)
    {
        if (CloseFile != null) CloseFile(this, e);
    }
    protected virtual void OnError(ErrorEventArgs e)
    {
        if (Error != null) Error(this, e);
    }

    #endregion
}
