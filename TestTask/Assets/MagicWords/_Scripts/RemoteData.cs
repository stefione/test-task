using System;
using System.Collections.Generic;

namespace TestTask.MagicWords
{
    public class RemoteData
    {
        public List<DialogData> dialogue;
        public List<AvatarData> avatars;

        public bool IsValid(Action<string> onError)
        {
            if(dialogue==null || dialogue.Count == 0)
            {
                string error = $"{nameof(dialogue)} is null or empty";
                onError?.Invoke(error);
                return false;
            }

            if (avatars == null || avatars.Count == 0)
            {
                string error = $"{nameof(avatars)} is null or empty";
                onError?.Invoke(error);
                return false;
            }

            foreach(var dialogLine in dialogue)
            {
                if (!dialogLine.IsValid(onError))
                {
                    return false;
                }
            }

            foreach (var avatar in avatars)
            {
                if (!avatar.IsValid(onError))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
