namespace appProveedores.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateTable1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Nombre", c => c.String());
            AddColumn("dbo.AspNetUsers", "Apellidos", c => c.String());
            AddColumn("dbo.AspNetUsers", "Empresa", c => c.String());
            AddColumn("dbo.AspNetUsers", "direccion", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "direccion");
            DropColumn("dbo.AspNetUsers", "Empresa");
            DropColumn("dbo.AspNetUsers", "Apellidos");
            DropColumn("dbo.AspNetUsers", "Nombre");
        }
    }
}
