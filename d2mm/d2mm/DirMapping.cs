using System;

namespace de.sebastianrutofski.d2mm
{
    public struct DirMapping
    {
        public bool Equals(DirMapping other)
        {
            return string.Equals(ModDir, other.ModDir) && string.Equals(DotaDir, other.DotaDir);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ModDir != null ? ModDir.GetHashCode() : 0)*397) ^ (DotaDir != null ? DotaDir.GetHashCode() : 0);
            }
        }

        public static bool operator ==(DirMapping left, DirMapping right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DirMapping left, DirMapping right)
        {
            return !left.Equals(right);
        }

        private string _ModDir;
        private string _DotaDir;

        public DirMapping(string modDir, string dotaDir) : this()
        {

            ModDir = modDir;
            DotaDir = dotaDir;
        }

        public string ModDir
        {
            get { return _ModDir; }
            set
            {
                if (ModDir == null)
                    _ModDir = String.Empty;
                if(ModDir != null & !ModDir.Equals(value))
                {
                    _ModDir = value;
                }
            }
        }

        public string DotaDir
        {
            get { return _DotaDir; }
            set
            {
                if (DotaDir == null)
                    _DotaDir = String.Empty;
                if(DotaDir != null & !DotaDir.Equals(value))
                {
                    _DotaDir = value;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is DirMapping && Equals((DirMapping) obj);
        }
    }
}