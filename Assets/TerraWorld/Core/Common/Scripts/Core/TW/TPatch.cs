namespace TerraUnity.Edittime
{
    public class TPatch 
    {
        private int _density = 8;
        private int _width = 10;
        private int _length = 10;
        private TGlobalPoint center;

        public TPatch(TGlobalPoint center, int width , int length)
        {
            this.center = center;
            _width = width;
            _length = length;
        }

        public int Density { get => _density; }
        public TGlobalPoint Center { get => center; set => center = value; }
        public int Width { get => _width; }
        public int Length { get => _length;}
    }
}

