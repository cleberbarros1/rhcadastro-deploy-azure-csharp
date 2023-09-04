using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using desafio_de_projeto_deploy_azure.Models;
using Microsoft.EntityFrameworkCore;

namespace desafio_de_projeto_deploy_azure.Context
{
    public class FuncionariosContext : DbContext
    {
        public FuncionariosContext(DbContextOptions<FuncionariosContext> options): base(options){}

        public DbSet<Funcionario> Funcionarios {get; set;}
        
    }
}