namespace appProveedores.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Productos : DbMigration
    {
        public override void Up()
        {
            AddColumn("Pedido", "calificacion", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("Pedido", "calificacion");
        }
    }
}
