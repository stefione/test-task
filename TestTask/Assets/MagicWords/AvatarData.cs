namespace TestTask.MagicWords
{
    public class AvatarData
    {
        public string name;
        public string url;
        public AvatarPosition avatarPosition;

        public string GetId()
        {
            return name + avatarPosition.ToString();
        }
    }
}
