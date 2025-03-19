using System;

namespace EduMastersAPI.Models
{
    public class Application
    {
        public int Id { get; set; }
        public int StudentId { get; set; } // معرف الطالب
        public string Degree { get; set; } // الدرجة (بكالوريوس، ماستر، دكتوراه)
        public int UniversityId { get; set; } // معرف الجامعة
        public string Language { get; set; } // اللغة (إنجليزي، تركي)
        public int BranchId { get; set; } // معرف الفرع
        public int CreatedByUserId { get; set; } // معرف المستخدم الذي أضاف التطبيق
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // تاريخ إنشاء التطبيق
        public string Status { get; set; } // حالة التطبيق (المرحلة الأولى، المرحلة الثانية، المرحلة الثالثة)

        // العلاقات مع الكائنات الأخرى
        public virtual Student Student { get; set; }
        public virtual University University { get; set; }
        public virtual UniversityBranch Branch { get; set; }
        public virtual User CreatedByUser { get; set; }
    }
}
