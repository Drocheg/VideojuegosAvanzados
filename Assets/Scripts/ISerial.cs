

public interface ISerial 
{
	void Serialize(BitWriter writer);
	void Deserialize(BitReader reader);

}