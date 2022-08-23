using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;

namespace Texemon.Scenes.ConversationScene
{
    public class ConversationRecord
    {
        public string Name { get; set; }
        public string Background { get; set; }
        public DialogueRecord[] DialogueRecords { get; set; }
        public string[] EndScript { get; set; }        
    }

    public class DialogueRecord
    {
        public string Speaker { get; set; }
        public string Text { get; set; }
        public string[] Script { get; set; }
    }
}
