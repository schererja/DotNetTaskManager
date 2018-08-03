namespace Manager.LIBS.Model
{
    /// <summary>
    ///     The message's Category
    /// </summary>
    public enum Category
    {
        Error,
        Command,
        TestResult,
        Start,
        Regex,
        FinalResult,
        Stress
    }

    /// <summary>
    ///     Class abstraction for Message in a Task (TaskModel)
    /// </summary>
    public class MessageModel
    {
        /// <summary>
        ///     The Category of the Message.
        /// </summary>
        public Category Category { get; set; }

        /// <summary>
        ///     The Message itself
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     The Message's TimeStamp
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        ///     Override for MessageModel ToString.
        /// </summary>
        /// <returns>Returns this Message as a string in JSON Format</returns>
        public override string ToString()
        {
            string catStr;
            switch (Category)
            {
                case Category.Error:
                    catStr = "ERROR";
                    break;
                case Category.Command:
                    catStr = "COMMAND";
                    break;
                case Category.TestResult:
                    catStr = "TESTRESULT";
                    break;
                case Category.Start:
                    catStr = "START";
                    break;
                case Category.Regex:
                    catStr = "REGEX";
                    break;
                case Category.FinalResult:
                    catStr = "FINALRESULT";
                    break;
                case Category.Stress:
                    catStr = "STRESS";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var result = "{\"Category\":\"" + catStr + "\", \"Message\":\"" +
                         Message + "\", \"TimeStamp\":\"" + TimeStamp + "\"}";

            return result;
        }
    }
}