#if UNITY_EDITOR
using System.Collections.Generic;
using System.Xml;

namespace TerraUnity.Edittime
{
    public class LatLonNode
    {
        public long id;
        public double lat, lon;

        public LatLonNode()
        {
        }

        public LatLonNode(long ID, double LAT, double LON)
        {
            id = ID;
            lat = LAT;
            lon = LON;
        }
    }

    public class Way
    {
        public long id;
        public List<long> wNodes;
        public Dictionary<string, string> _tags;

        public Way()
        {
            id = 0;
            wNodes = new List<long>();
            _tags = new Dictionary<string, string>();
        }

        public Way(long ID)
        {
            id = ID;
            wNodes = new List<long>();
            _tags = new Dictionary<string, string>();
        }

        public string key(string Keyword)
        {
            string result = "";

            if (_tags != null)
            {
                _tags.TryGetValue(Keyword, out result);

            }
            return result;
        }
    }

    public class Relation
    {
        public long id;
        public List<Member> members;
        public Dictionary<string, string> tags;

        public Relation()
        {
            id = 0;
            members = new List<Member>();
            tags = new Dictionary<string, string>();
        }

        public Relation(long ID)
        {
            id = ID;
            members = new List<Member>();
            tags = new Dictionary<string, string>();
        }

        public string key(string Keyword)
        {
            string result = "";

            if (tags != null)
            {
                tags.TryGetValue(Keyword, out result);
            }

            return result;
        }
    }

    public struct Member
    {
        public string type;
        public long nodeRef;
        public string role;
    }

}

#endif