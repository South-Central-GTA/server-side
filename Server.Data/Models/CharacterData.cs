using AltV.Net;
using Server.Database.Models.Character;

namespace Server.Data.Models;

public class CharacterData : IWritable
{
    public CharacterModel Character { get; }
    public ClothingsData Clothings { get; }
    
    public CharacterData(CharacterModel character, ClothingsData clothings)
    {
        Character = character;
        Clothings = clothings;
    }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(CharacterData data, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("character");

        CharacterModel.Serialize(data.Character, writer);

        writer.Name("clothings");

        ClothingsData.Serialize(data.Clothings, writer);

        writer.EndObject();
    }
}