using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Manager.LIBS.Model
{
    /// <summary>
    ///     Class abstraction of a Task
    /// </summary>
    public class TaskModel
    {
        /// <summary>
        ///     Constructor for Task Model
        /// </summary>
        public TaskModel()
        {
            var message = new MessageModel
            {
                Category = Category.Error,
                Message = "Base Task is unused",
                TimeStamp = DateTime.Now
            };
            Guid = Guid.NewGuid();
            Name = "Unknown";
            Priority = 1000;
            WhereFrom = "Unknown";
            WhereTo = "ERROR";
            TimeStamp = DateTime.Now;
            SerialNumber = 0000000;
            Messages = new List<MessageModel>
            {
                message
            };
        }

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="TaskModel" /> class with a string of the task
        /// </summary>
        /// <param name="task">
        ///     The Task as a string in JSON Format to convert to a Task Model
        /// </param>
        /// <exception cref="NullReferenceException">
        ///     Throws Exception when the following is empty or null:
        ///     Name
        ///     WhereTo
        ///     WhereFrom
        /// </exception>
        public TaskModel(string task)
        {
            if (!IsValidJson(task)) return;
            if (task == "")
            {
                var message = new MessageModel
                {
                    Category = Category.Error,
                    Message = "No String sent to constructor",
                    TimeStamp = DateTime.Now
                };
                Guid = Guid.NewGuid();
                Name = "No String Error";
                Priority = 1;
                WhereFrom = "";
                WhereTo = "ERROR";
                Timeout = 0;
                TimeStamp = DateTime.Now;
                SerialNumber = 0000000;
                Messages = new List<MessageModel>
                {
                    message
                };
            }
            else
            {
                var taskJObj = JObject.Parse(task);

                if (Guid.TryParse(taskJObj.GetValue("Guid").ToString(),
                    out var uuid))
                    Guid = uuid;

                if (string.IsNullOrEmpty(
                    taskJObj.GetValue("Name")
                        .ToString()
                ))
                    throw new NullReferenceException();
                Name = taskJObj.GetValue("Name").ToString();

                if (string.IsNullOrEmpty(
                    taskJObj.GetValue("SerialNumber")
                        .ToString()
                ))
                    throw new NullReferenceException();

                SerialNumber = uint.Parse(taskJObj.GetValue("SerialNumber").ToString());

                if (string.IsNullOrEmpty(
                    taskJObj.GetValue("WhereFrom")
                        .ToString()
                ))
                    throw new NullReferenceException();
                WhereFrom = taskJObj.GetValue("WhereFrom").ToString();

                if (string.IsNullOrEmpty(
                    taskJObj.GetValue("WhereTo")
                        .ToString()
                ))
                    throw new NullReferenceException();

                WhereTo = taskJObj.GetValue("WhereTo").ToString();

                if (decimal.TryParse(taskJObj.GetValue("Priority").ToString(),
                    out var number))
                    Priority = number;
                if (DateTime.TryParse(taskJObj.GetValue("TimeStamp").ToString(),
                    out var dt))
                    TimeStamp = dt;
                if (int.TryParse(taskJObj.GetValue("Timeout").ToString(),
                    out var timeout))
                    Timeout = timeout;
                Messages = JsonConvert.DeserializeObject<List<MessageModel>>(
                    taskJObj.GetValue("Messages").ToString());
            }
        }

        /// <summary>
        ///     Global Unigue Identifier for the Task
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        ///     The Name for the Task
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The Priority for the Task
        /// </summary>
        public decimal Priority { get; set; }

        /// <summary>
        ///     The Where From for the Task
        /// </summary>
        public string WhereFrom { get; set; }

        /// <summary>
        ///     The Where To For the Task
        /// </summary>
        public string WhereTo { get; set; }

        /// <summary>
        ///     The TimeStamp for the Task
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        ///     The timeout for a task, used for how long before a timeout of a task
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        ///     The Serial Number associated with the task
        /// </summary>
        public uint SerialNumber { get; set; }

        /// <summary>
        ///     The Messages that are associated with the given Task
        /// </summary>
        public List<MessageModel> Messages { get; set; }

        /// <summary>
        ///     Creates the string in JSON format of the Task Model
        /// </summary>
        /// <returns>
        ///     This Task Model as a string
        /// </returns>
        public override string ToString()
        {
            var result = "{\"Guid\":\"" + Guid +
                         "\", \"Name\":\"" + Name +
                         "\", \"SerialNumber\":\"" + SerialNumber +
                         "\", \"Priority\":" + Priority +
                         ", \"WhereFrom\":\"" + WhereFrom +
                         "\", \"WhereTo\":\"" + WhereTo +
                         "\", \"Timeout\":\"" + Timeout +
                         "\", \"TimeStamp\":\"" + TimeStamp +
                         "\", \"Messages\":[";
            // Null reference can be tripped
            var i = 1;
            foreach (var message in Messages)
            {
                if ((Messages.Count > 1) & (Messages.Count != i))
                    result += message + ",";
                else if (Messages.Count == i)
                    result += message.ToString();
                else
                    result += message.ToString();

                i++;
            }

            result += "]}";
            result += '\0';

            return result;
        }

        private static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            try
            {
                var obj = JToken.Parse(strInput);
                return true;
            }
            catch (JsonReaderException jex)
            {
                //Exception in parsing json
                Console.WriteLine(jex.Message);
                return false;
            }
            catch (Exception ex) //some other exception
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}