namespace Manager.LIBS.Model
{
    /// <summary>
    ///     TaskQueue Class abstraction of Concurrent Bag of Task(s)
    /// </summary>
    public class TaskQueue
    {
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="TaskQueue" /> class. Creates a new Concurrent
        ///     Bag
        /// </summary>
        public TaskQueue()
        {
            Queue = new ConcurrentBag<TaskModel>();
        }

        /// <summary>
        ///     The Concurrent Bag of Task(s) (Queue)
        /// </summary>
        public ConcurrentBag<TaskModel> Queue { get; set; }
    }
}