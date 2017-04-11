using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToDoClient.Models
{
    public class ToDoItemViewModel
    {
        /// <summary>
        /// Gets or sets to do identifier.
        /// </summary>
        /// <value>
        /// To do identifier.
        /// </value>
        public int ToDoId { get; set; }


        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this todo-item is completed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this todo-item is completed; otherwise, <c>false</c>.
        /// </value>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Gets or sets the name (description) of todo-item.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
    }
}