namespace WoWonder.Activities.Live.Stats
{
    public class StatsData
    {
        private long Uid;
        private int Width;
        private int Height;
        private int Framerate;
        private string RecvQuality;
        private string SendQuality;

        public long GetUid()
        {
            return Uid;
        }

        public void SetUid(long uid)
        {
            this.Uid = uid;
        }

        public int GetWidth()
        {
            return Width;
        }

        public void SetWidth(int width)
        {
            this.Width = width;
        }

        public int GetHeight()
        {
            return Height;
        }

        public void SetHeight(int height)
        {
            this.Height = height;
        }

        public int GetFramerate()
        {
            return Framerate;
        }

        public void SetFramerate(int framerate)
        {
            this.Framerate = framerate;
        }

        public string GetRecvQuality()
        {
            return RecvQuality;
        }

        public void SetRecvQuality(string recvQuality)
        {
            this.RecvQuality = recvQuality;
        }

        public string GetSendQuality()
        {
            return SendQuality;
        }

        public void SetSendQuality(string sendQuality)
        {
            this.SendQuality = sendQuality;
        }
    }

}