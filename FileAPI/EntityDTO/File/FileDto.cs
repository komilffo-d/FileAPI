using Database.Enums;

namespace FileAPI.EntityDTO.File
{
    public record class FileDto(int id,string FileName, FileType FileType);
}
