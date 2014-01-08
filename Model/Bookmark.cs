using System;
using System.Collections.Generic;

namespace Model
{
    public class Bookmark
    {
        private Selection selection;
        public Selection Selection
        {
            get { return selection; }
            set { selection = value; }
        }

        private string note;
        public string Note
        {
            get { return note; }
            set
            {
                note = value;
                this.last_modified_time = DateTime.Now;
            }
        }

        private DateTime created_time;
        public DateTime CreatedTime
        {
            get { return created_time; }
            set { created_time = value; }
        }

        private DateTime last_modified_time;
        public DateTime LastModifiedTime
        {
            get { return last_modified_time; }
            set { last_modified_time = value; }
        }

        public Bookmark(Selection selection, string note)
        {
            this.selection = selection;
            this.note = note;
            this.created_time = DateTime.Now;
            this.last_modified_time = DateTime.Now;
        }
    }
}
