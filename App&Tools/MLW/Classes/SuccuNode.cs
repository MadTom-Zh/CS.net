using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MLW_Succubus_Storys.Classes
{
    public class SuccuNode
    {
        public BitmapImage succuIcon;
        public string succuName;
        public string succuNameLocalized;
        public BitmapImage mtl1Icon;
        public BitmapImage mtl2Icon;
        public BitmapImage mtl3Icon;
        public string mtl1Name;
        public string mtl2Name;
        public string mtl3Name;


        public List<StoryNode> storyLv1 = new List<StoryNode>();
        public List<StoryNode> storyLv2 = new List<StoryNode>();
        public List<StoryNode> storyLv3 = new List<StoryNode>();
        public EndingNode endingA, endingB;


        public class StoryNode
        {
            public Guid id = Guid.NewGuid();
            public string name;
            public ChatHistory chatHistory = new ChatHistory();
            public List<ChoiceNode> choices = new List<ChoiceNode>();
        }

        public class ChoiceNode
        {
            public Guid id = Guid.NewGuid();
            public string name,text;
            public ChatHistory chatHistory = new ChatHistory();
            public List<ChoiceNode> nextChoices = new List<ChoiceNode>();
            public string nextNodeName = "";
            public StoryNode nextStory ;
            public EndingNode nextEnding;
        }

        public class ChatHistory
        {
            public List<ChatMsg> chatMsgs = new List<ChatMsg>();
            public struct ChatMsg
            {
                public bool fromSucOrPlayer;
                public string msg;
                public string args;
            }
        }

        public class EndingNode
        {
            public bool isGoodOrBad;
            public BitmapImage image;
            public List<string> msgs = new List<string>();
        }
    }
}
