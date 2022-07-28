using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Models
{
    public class ConversationModel
    {
        public string Name { get; set; }
        public string Background { get; set; }
        public DialogueData[] DialogueData { get; set; }
        public string[] EndScript { get; set; }

        public static List<ConversationModel> Models { get; set; }
    }

    public class DialogueData
    {
        public string Speaker { get; set; }
        public string Text { get; set; }
        public string[] Script { get; set; }
    }
}
