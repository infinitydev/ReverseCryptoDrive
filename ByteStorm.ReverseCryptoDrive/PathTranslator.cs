using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ByteStorm.PassthroughDrive
{
    class PathTranslator
    {
        private readonly BPlusTree<string, string> tree;
        private static readonly string ID_ALLOWED_CHARS = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private static readonly int ID_MIN_LENGTH = 3;
        private static readonly int ID_MAX_LENGTH = 15;
        private static readonly string ID_PREFIX = "i";
        private static readonly string NAME_PREFIX = "n";
        private bool isDisposed;

        public PathTranslator(string dbpath)
        {
            BPlusTree<string, string>.OptionsV2 options = new BPlusTree<string, string>.OptionsV2(new PrimitiveSerializer(), new PrimitiveSerializer());
            options.CalcBTreeOrder(24, 24);
            options.CreateFile = CreatePolicy.IfNeeded;
            options.FileName = dbpath;
            options.CacheKeepAliveMaximumHistory = 20000;
            options.TransactionLogLimit = 10 * 1024 * 1024;

            tree = new BPlusTree<string, string>(options);
        }

        public string getOrCreateIdFor(string name, Random rng)
        {
            // try to get the stored id for the supplied name
            string id = getIdFor(name);
            if (id != null)
                return id;

            // if no rng was supplied, we cannot generate an id
            if (rng == null)
                return null;

            // no id stored for name => generate one and store id<=>name association
            id = getUniqueId(rng);
            setIdNameMapping(id, name);
            return id;
        }

        public string getIdFor(string name)
        {
            string s;
            if (tree.TryGetValue(NAME_PREFIX + name, out s))
                return s;
            else
                return null;
        }

        public string getNameFor(string id)
        {
            string s;
            if (tree.TryGetValue(ID_PREFIX + id, out s))
                return s;
            else
                return null;
        }

        public void commit()
        {
            if (!isDisposed)
                tree.Commit();
        }

        public void close()
        {
            isDisposed = true;
            tree.Dispose();
        }

        protected void setIdNameMapping(string id, string name)
        {
            KeyValuePair<string, string>[] kv = new KeyValuePair<string, string>[2];
            kv[0] = new KeyValuePair<string, string>(ID_PREFIX+id, name);
            kv[1] = new KeyValuePair<string, string>(NAME_PREFIX+name, id);
            tree.AddRange(kv, false);
        }

        protected string getUniqueId(Random rng)
        {
            string id;
            do
            {
                id = generateRandomString(ID_ALLOWED_CHARS, ID_MIN_LENGTH, ID_MAX_LENGTH, rng);
            } while (tree.ContainsKey(ID_PREFIX + id));

            return id;
        }

        private static string generateRandomString(string allowedChars, int minLength, int maxLength, Random rng)
        {
            char[] chars = new char[maxLength];
            int setLength = allowedChars.Length;

            int length = rng.Next(minLength, maxLength + 1);

            for (int i = 0; i < length; ++i)
            {
                chars[i] = allowedChars[rng.Next(setLength)];
            }

            return new string(chars, 0, length);
        }
    }
}
