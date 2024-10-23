using System.ComponentModel.DataAnnotations;

namespace Localization.SqlLocalizer.DbStringLocalizer
{
    public class LocalizationComment
    {
        #region Properties
        [Required]
        public long Id { get; set; }

        [Required]
        public long LocalizationRecordId { get; set; }

        [Required]
        public string Text { get; set; }
        #endregion

        #region Navigation Properties
        public virtual LocalizationRecord LocalizationRecord { get; set; }
        #endregion
    }
}
