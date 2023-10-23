using Database.Enums;
using Newtonsoft.Json;

namespace FileAPI.EntityDTO.File
{
    [JsonObject(MemberSerialization.OptIn)]
    public record class FileUploadDto(IFormFile FileDetails, FileType FileType);
}
