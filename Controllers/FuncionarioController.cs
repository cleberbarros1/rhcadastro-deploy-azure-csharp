using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using desafio_de_projeto_deploy_azure.Context;
using desafio_de_projeto_deploy_azure.Models;
using Microsoft.AspNetCore.Mvc;
using Azure.Data.Tables;
using System.Text.Json;

namespace desafio_de_projeto_deploy_azure.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FuncionarioController : ControllerBase
    {
        public readonly FuncionariosContext _context;
        private readonly string _connectionStringTable;
        private readonly string _tableName;

        public FuncionarioController(FuncionariosContext context, IConfiguration configuration){
            this._context = context;
            this._connectionStringTable = configuration.GetValue<string>("SAConnectionString");
            this._tableName = configuration.GetValue<string>("AzureTableName");
        }

        [HttpGet]
        public IActionResult ObterFuncionarios(){
            var funcionarios = this._context.Funcionarios.ToList();

            return Ok(funcionarios);
        }

        [HttpGet("{id}")]
        public IActionResult ObterFuncionario(int id){
            var funcionario = this._context.Funcionarios.Find(id);

            return Ok(funcionario);
        }

        [HttpPut("{id}")]
        public IActionResult AtualizarFuncionario(int id, string nome, string endereco, string ramal, string emailProfissional, string departamento, decimal salario){

            var _funcionario = this._context.Funcionarios.Find(id);

            if(_funcionario == null) { return NotFound();}

            if(nome != null) { _funcionario.Nome = nome;}
            if(endereco != null) { _funcionario.Endereco = endereco;}
            if(ramal != null) { _funcionario.Ramal = ramal;}
            if(emailProfissional != null) { _funcionario.EmailProfissional = emailProfissional;}
            if(departamento != null) { _funcionario.Departamento = departamento;}
            if(salario != 0) { _funcionario.Salario = salario;}
            this._context.Funcionarios.Update(_funcionario);
            this._context.SaveChanges();

            //Registro de Log de rastreio
            TableClient _logTable = GetTableCliente();

            FuncionarioLog funclog = new FuncionarioLog();
            funclog.RowKey = Guid.NewGuid().ToString();
            funclog.PartitionKey = _funcionario.Departamento;
            funclog.IdFunc = _funcionario.Id;
            funclog.Nome = _funcionario.Nome;
            funclog.Endereco = _funcionario.Endereco;
            funclog.Ramal = _funcionario.Ramal;
            funclog.EmailProfissional = _funcionario.EmailProfissional;
            funclog.Departamento = _funcionario.Departamento;
            funclog.Salario = _funcionario.Salario;
            funclog.DataAdmissao = _funcionario.DataAdmissao.ToUniversalTime();
            funclog.TipoAcao = "Atualização";
            funclog.json = JsonSerializer.Serialize(_funcionario).ToString();

            _logTable.UpsertEntity(funclog);
            

            return Ok(_funcionario);
        }

        [HttpDelete("{id}")]
        public IActionResult DeletarFuncionario(int id){

            var funcionario = this._context.Funcionarios.Find(id);

            if(funcionario == null) { return NotFound();}
            
            this._context.Funcionarios.Remove(funcionario);
            this._context.SaveChanges();

            //Registro de Log de rastreio
            var _logTable = GetTableCliente();
            FuncionarioLog funclog = new FuncionarioLog();

            funclog.RowKey = Guid.NewGuid().ToString();
            funclog.PartitionKey = funcionario.Departamento;
            funclog.IdFunc = funcionario.Id;
            funclog.Nome = funcionario.Nome;
            funclog.Endereco = funcionario.Endereco;
            funclog.Ramal = funcionario.Ramal;
            funclog.EmailProfissional = funcionario.EmailProfissional;
            funclog.Departamento = funcionario.Departamento;
            funclog.Salario = funcionario.Salario;
            funclog.DataAdmissao = funcionario.DataAdmissao.ToUniversalTime();
            funclog.TipoAcao = "Eliminação";
            funclog.json = JsonSerializer.Serialize(funcionario).ToString();

            _logTable.UpsertEntity(funclog);

            return Ok(funcionario);
        }

        [HttpPost]
        public IActionResult CriarFuncionario(Funcionario funcionario){

            funcionario.DataAdmissao = DateTime.Now;

            this._context.Funcionarios.Add(funcionario);
            this._context.SaveChanges();

            //Registro de Log de rastreio
            var _logTable = GetTableCliente();
            FuncionarioLog funclog = new FuncionarioLog();

            funclog.RowKey = Guid.NewGuid().ToString();
            funclog.PartitionKey = funcionario.Departamento;
            funclog.IdFunc = funcionario.Id;
            funclog.Nome = funcionario.Nome;
            funclog.Endereco = funcionario.Endereco;
            funclog.Ramal = funcionario.Ramal;
            funclog.EmailProfissional = funcionario.EmailProfissional;
            funclog.Departamento = funcionario.Departamento;
            funclog.Salario = funcionario.Salario;
            funclog.DataAdmissao = funcionario.DataAdmissao.ToUniversalTime();
            funclog.TipoAcao = "Criação";
            funclog.json = JsonSerializer.Serialize(funcionario).ToString();

            _logTable.UpsertEntity(funclog);

            return Ok(funcionario);
        
        }

        private TableClient GetTableCliente(){
            var serviceCliente = new TableServiceClient(_connectionStringTable);
            var _myLogTable = serviceCliente.GetTableClient(_tableName);

            _myLogTable.CreateIfNotExists();

            return _myLogTable;

        }
    }
}