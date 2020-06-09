using System;

namespace MessageClasses
{
    [Serializable]
    public class Participant
    {
        public string name;
        public int id;

        public Participant(string name, int id)
        {
            this.name = name;
            this.id = id;
        }
    }
}
