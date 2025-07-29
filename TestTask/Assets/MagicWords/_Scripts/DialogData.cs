using System;

namespace TestTask.MagicWords
{
    public class DialogData
    {
        public string name;
        public string text;

        public bool IsValid(Action<string> onError)
        {
            if (string.IsNullOrEmpty(name))
            {
                string error = $"{GetType()} {nameof(name)} is null or empty";
                onError?.Invoke(error);
                return false;
            }

            if (string.IsNullOrEmpty(text))
            {
                string error = $"{GetType()} {nameof(text)} is null or empty";
                onError?.Invoke(error);
                return false;
            }

            return true;
        }
    }
}
