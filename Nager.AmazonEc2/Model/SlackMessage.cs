namespace Nager.AmazonEc2.Model
{
    public class SlackMessage
    {
        public string Channel { get; set; }
        public string IconEmoji { get; set; }
        public string Username { get; set; }
        public string Text { get; set; }

        public SlackMessage()
        { }

        public SlackMessage(string channel, string iconEmoji, string username, string text)
        {
            this.Channel = channel;
            this.IconEmoji = iconEmoji;
            this.Username = username;
            this.Text = text;
        }
    }
}
