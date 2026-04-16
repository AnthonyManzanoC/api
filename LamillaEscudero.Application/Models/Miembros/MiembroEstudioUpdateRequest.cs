

namespace LamillaEscudero.Application.Models.Miembros
{
    public class MiembroEstudioUpdateRequest : MiembroEstudioCreateRequest
    {
        public bool IsActive { get; set; } = true;
    }

}
