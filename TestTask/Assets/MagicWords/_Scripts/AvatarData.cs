using System;

namespace TestTask.MagicWords
{
    public class AvatarData
    {
        public string name;
        public string url;
        public AvatarPosition avatarPosition;

        public bool IsValid(Action<string> onError)
        {
            if (string.IsNullOrEmpty(name))
            {
                string error = $"{GetType()} {nameof(name)} is null or empty";
                onError?.Invoke(error);
                return false;
            }

            if (string.IsNullOrEmpty(url))
            {
                string error = $"{GetType()} {nameof(url)} is null or empty";
                onError?.Invoke(error);
                return false;
            }

            return true;
        }
    }
}
