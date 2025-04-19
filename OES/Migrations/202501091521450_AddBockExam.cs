namespace OES.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBockExam : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BlockExams",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        ExamId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.BlockExams");
        }
    }
}
