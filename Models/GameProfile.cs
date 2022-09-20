using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Texemon.Main;
using Texemon.Scenes.StatusScene;

namespace Texemon.Models
{
    public static class GameProfile
    {
        [Serializable]
        public class MapState
        {
            public string name;
            public int seed;
        }

        public const string SAVE_FOLDER = "\\Save";
        private static Dictionary<string, object> DEFAULT_SAVE_VALUES = new Dictionary<string, object>()
        {
            { "NewPlayer", true }
        };

        private static int saveSlot;
        private static Dictionary<string, object> saveData;
        private static PlayerProfile playerProfile;
        private static ModelCollection<ItemRecord> inventory;

        public static void NewState()
        {
            //saveSlot = -1;
            saveSlot = 0;
            saveData = new Dictionary<string, object>(DEFAULT_SAVE_VALUES);
            playerProfile = new PlayerProfile();
            inventory = new ModelCollection<ItemRecord>();
        }

        public static void LoadState(string saveFileName)
        {
            int slotStart = saveFileName.LastIndexOf(SAVE_FOLDER) + 5;
            int slotEnd = saveFileName.LastIndexOf('.');
            saveSlot = int.Parse(saveFileName.Substring(slotStart, slotEnd - slotStart));

            string savePath = CrossPlatformGame.SETTINGS_DIRECTORY + SAVE_FOLDER + "//" + saveFileName;

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileInfo fileInfo = new FileInfo(savePath);

            using (FileStream fileStream = fileInfo.OpenRead())
            {
                saveData = (Dictionary<string, object>)binaryFormatter.Deserialize(fileStream);
                playerProfile = (PlayerProfile)binaryFormatter.Deserialize(fileStream);
                inventory = (ModelCollection<ItemRecord>)binaryFormatter.Deserialize(fileStream);
            }
        }

        public static void SaveState()
        {
            if (saveSlot == -1)
            {
                List<string> saveSlots = SaveList;
                saveSlot = 0;

                while (saveSlots.Exists(x => ParseSlotNumber(x) == saveSlot))
                    saveSlot++;
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            string directory = CrossPlatformGame.SETTINGS_DIRECTORY + SAVE_FOLDER;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream fileStream = File.Open(directory + "\\Save" + saveSlot + ".sav", FileMode.OpenOrCreate))
            {
                binaryFormatter.Serialize(fileStream, saveData);
                fileStream.Flush();

                binaryFormatter.Serialize(fileStream, playerProfile);
                fileStream.Flush();

                binaryFormatter.Serialize(fileStream, inventory);
                fileStream.Flush();
            }
        }

        public static void DeleteSave(int slot)
        {
            File.Delete(CrossPlatformGame.SETTINGS_DIRECTORY + SAVE_FOLDER + "\\" + slot + ".sav");
        }

        public static List<Dictionary<string, object>> GetAllSaveData()
        {
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();

            string savePath = CrossPlatformGame.SETTINGS_DIRECTORY + SAVE_FOLDER;
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            foreach (string saveFile in Directory.GetFiles(savePath).Where(x => Path.GetExtension(x) == ".sav"))
            {
                Dictionary<string, object> saveData;
                FileInfo fileInfo = new FileInfo(savePath);
                using (FileStream fileStream = fileInfo.OpenRead())
                {
                    saveData = (Dictionary<string, object>)binaryFormatter.Deserialize(fileStream);
                }

                results.Add(saveData);
            }

            return results;
        }

        public static void SetSaveData<T>(string name, T value)
        {
            if (saveData.ContainsKey(name)) saveData[name] = value;
            else saveData.Add(name, value);
        }

        public static T GetSaveData<T>(string name)
        {
            if (saveData.ContainsKey(name)) return (T)saveData[name];

            T newValue = default(T);
            saveData.Add(name, newValue);
            return newValue;
        }

        public static int ParseSlotNumber(string saveName)
        {
            int slotStart = saveName.LastIndexOf(SAVE_FOLDER) + 6;
            int slotEnd = saveName.LastIndexOf('.');

            return int.Parse(saveName.Substring(slotStart, slotEnd - slotStart));
        }

        public static PlayerProfile PlayerProfile { get => playerProfile; }
        public static ModelCollection<ItemRecord> Inventory { get => inventory; }

        public static List<string> SaveList
        {
            get
            {
                if (!Directory.Exists(CrossPlatformGame.SETTINGS_DIRECTORY + SAVE_FOLDER)) Directory.CreateDirectory(CrossPlatformGame.SETTINGS_DIRECTORY + SAVE_FOLDER);
                return Directory.GetFiles(CrossPlatformGame.SETTINGS_DIRECTORY + SAVE_FOLDER).ToList().FindAll(x => x.Substring(x.IndexOf('.'), 4) == ".sav");
            }
        }
    }
}
