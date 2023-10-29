using System.Collections.Concurrent;

namespace FileAPI.Misc.File
{

    public class FileResultContainerService
    {
        private ConcurrentDictionary<Guid, double> fileResultDictionary = new ConcurrentDictionary<Guid, double>();

        public double? Read(Guid identity)
        {
            if (fileResultDictionary.TryGetValue(identity, out double value))
                return value;
            else return null;
        }

        public void Write(Guid identity, double value)
        {
            fileResultDictionary.AddOrUpdate(identity, value, (key, oldValue) => value);

        }

        public double Delete(Guid identity)
        {
            fileResultDictionary.TryRemove(identity, out double value);
            return value;
        }

    }
}
