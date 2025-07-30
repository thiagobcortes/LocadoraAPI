CREATE DATABASE locadora_db;

USE locadora_db;

-- -----------------------------------------------------
-- Tabela CLIENTES
-- -----------------------------------------------------
CREATE TABLE CLIENTES (
    cpf CHAR(11) PRIMARY KEY,
    nome VARCHAR(150) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    telefone VARCHAR(15),

    -- Validação de formato para cada campo
    CONSTRAINT chk_cpf CHECK (cpf REGEXP '^[0-9]{11}$'),
    CONSTRAINT chk_nome CHECK (nome REGEXP '^[A-Za-z ]+$'),
    CONSTRAINT chk_email CHECK (email REGEXP '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$'),
    CONSTRAINT chk_telefone CHECK (telefone REGEXP '^[0-9]+$')
);

-- Insere um cliente
INSERT INTO CLIENTES (cpf, nome, email, telefone)
VALUES ('12345678901', 'Joao da Silva', 'joao.silva@email.com', '11987654321');


-- -----------------------------------------------------
-- Tabela VEICULOS
-- -----------------------------------------------------

CREATE TABLE VEICULOS (
    id_veiculo INT PRIMARY KEY AUTO_INCREMENT,
    placa CHAR(7) NOT NULL UNIQUE,
    marca VARCHAR(50) NOT NULL,
    modelo VARCHAR(50) NOT NULL,
    ano INT NOT NULL,
    cor ENUM('Branco', 'Preto', 'Prata') NOT NULL,
    preco_diaria DECIMAL(10, 2) NOT NULL,
    disponibilidade ENUM('Disponível', 'Indisponível') NOT NULL DEFAULT 'Disponível',

    -- Validações de formato
    CONSTRAINT chk_placa CHECK (placa REGEXP '^[A-Z0-9]{7}$'),
    CONSTRAINT chk_marca CHECK (marca REGEXP '^[A-Za-z ]+$'),
    CONSTRAINT chk_ano_veiculo CHECK (ano > 1990 AND ano < 2026),
    CONSTRAINT chk_preco_diaria_veiculo CHECK (preco_diaria > 0)
);

-- Insere veículos
INSERT INTO VEICULOS (placa, marca, modelo, ano, cor, preco_diaria, disponibilidade) VALUES
('ABC1234', 'Volkswagen', 'Gol', 2022, 'Branco', 120.00, 'Disponível'),
('DEF5678', 'Fiat', 'Mobi', 2023, 'Preto', 110.50, 'Disponível');


-- -----------------------------------------------------
-- Tabela FUNCIONARIOS
-- -----------------------------------------------------

CREATE TABLE FUNCIONARIOS (
    id_funcionario INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(150) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    telefone VARCHAR(15),

    -- Validações de formato 
    CONSTRAINT chk_funcionario_nome CHECK (nome REGEXP '^[A-Za-z ]+$'),
    CONSTRAINT chk_funcionario_email CHECK (email REGEXP '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$'),
    CONSTRAINT chk_funcionario_telefone CHECK (telefone REGEXP '^[0-9]+$')
);

-- Insere um funcionário
INSERT INTO FUNCIONARIOS (nome, email, telefone)
VALUES ('Jose Carlos', 'jose.carlos@locadora.com', '99999911');

-- -----------------------------------------------------
-- Tabela LOCAÇOES
-- -----------------------------------------------------

CREATE TABLE LOCACOES (
    id_locacao INT PRIMARY KEY AUTO_INCREMENT,
    cpf_cliente_fk CHAR(11) NOT NULL,
    id_funcionario_fk INT NOT NULL,
    data_inicio DATETIME NOT NULL,
    data_devolucao_prevista DATE NOT NULL,
    data_devolucao_efetiva DATETIME NULL, 
    preco_total DECIMAL(10, 2),

    CONSTRAINT fk_locacao_cliente FOREIGN KEY (cpf_cliente_fk) REFERENCES CLIENTES(cpf),
    CONSTRAINT fk_locacao_funcionario FOREIGN KEY (id_funcionario_fk) REFERENCES FUNCIONARIOS(id_funcionario)
);

-- -----------------------------------------------------
-- Tabela de Associação LOCACAO_VEICULOS
-- -----------------------------------------------------
CREATE TABLE LOCACAO_VEICULOS (
    id_locacao_fk INT NOT NULL,
    id_veiculo_fk INT NOT NULL,
    PRIMARY KEY (id_locacao_fk, id_veiculo_fk),
    CONSTRAINT fk_lv_locacao FOREIGN KEY (id_locacao_fk) REFERENCES LOCACOES(id_locacao),
    CONSTRAINT fk_lv_veiculo FOREIGN KEY (id_veiculo_fk) REFERENCES VEICULOS(id_veiculo)
);


-- -----------------------------------------------------
-- Tabela MANUTENCOES
-- -----------------------------------------------------
CREATE TABLE MANUTENCOES (
    id_manutencao INT PRIMARY KEY AUTO_INCREMENT,
    id_veiculo_fk INT NOT NULL,
    id_funcionario_fk INT NOT NULL,
    data_inicio DATE NOT NULL,
    data_fim DATE NULL,
    descricao TEXT,
    CONSTRAINT fk_manutencao_veiculo FOREIGN KEY (id_veiculo_fk) REFERENCES VEICULOS(id_veiculo),
    CONSTRAINT fk_manutencao_funcionario FOREIGN KEY (id_funcionario_fk) REFERENCES FUNCIONARIOS(id_funcionario)
);


-- -----------------------------------------------------
-- Cria a VIEW para o relatório de locações ativas
-- -----------------------------------------------------

CREATE VIEW vw_RelatorioLocacoesAtivas AS
SELECT
    l.id_locacao,
    l.data_inicio,
    l.data_devolucao_prevista,
    c.cpf AS cpf_cliente,
    c.nome AS nome_cliente,
    v.placa AS placa_veiculo,
    v.marca AS marca_veiculo,
    v.modelo AS modelo_veiculo,
    f.nome AS nome_funcionario
FROM
    LOCACOES l
JOIN
    CLIENTES c ON l.cpf_cliente_fk = c.cpf
JOIN
    LOCACAO_VEICULOS lv ON l.id_locacao = lv.id_locacao_fk
JOIN
    VEICULOS v ON lv.id_veiculo_fk = v.id_veiculo
JOIN
    FUNCIONARIOS f ON l.id_funcionario_fk = f.id_funcionario
WHERE
    l.data_devolucao_efetiva IS NULL; 


-- -----------------------------------------------------
-- PROCEDURE
-- -----------------------------------------------------
-- Define um novo delimitador para que possamos usar ';' dentro do procedimento
DELIMITER $$

CREATE PROCEDURE sp_RegistrarLocacao(
    IN p_cpf_cliente CHAR(11),
    IN p_id_funcionario INT,
    IN p_id_veiculo INT,
    IN p_data_devolucao_prevista DATE,
    OUT p_id_locacao_criada INT,
    OUT p_mensagem_erro VARCHAR(255)
)
BEGIN
    -- Declaração de variáveis locais
    DECLARE v_disponibilidade_veiculo VARCHAR(20);
    DECLARE v_veiculo_encontrado BOOLEAN DEFAULT TRUE;

    -- Este "Handler" é um manipulador de eventos. Se a query SELECT não encontrar nenhuma
    -- linha (NOT FOUND), ele será acionado e mudará a variável v_veiculo_encontrado para FALSE.
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET v_veiculo_encontrado = FALSE;

    -- Inicia a transação
    START TRANSACTION;

    -- Agora, a query é mais simples. Ela apenas busca a disponibilidade.
    -- O "Handler" acima cuidará do caso em que o veículo não é encontrado.
    SELECT disponibilidade INTO v_disponibilidade_veiculo
    FROM VEICULOS
    WHERE id_veiculo = p_id_veiculo FOR UPDATE;

    -- Lógica de validação usando a variável do Handler
    IF NOT v_veiculo_encontrado THEN
        SET p_mensagem_erro = 'Erro: Veículo não encontrado.';
        ROLLBACK;
    ELSEIF v_disponibilidade_veiculo != 'Disponível' THEN
        SET p_mensagem_erro = 'Erro: Veículo não está disponível para locação.';
        ROLLBACK;
    ELSE
        -- Se tudo estiver OK, executa as operações
        INSERT INTO LOCACOES (cpf_cliente_fk, id_funcionario_fk, data_inicio, data_devolucao_prevista)
        VALUES (p_cpf_cliente, p_id_funcionario, NOW(), p_data_devolucao_prevista);

        SET p_id_locacao_criada = LAST_INSERT_ID();

        INSERT INTO LOCACAO_VEICULOS (id_locacao_fk, id_veiculo_fk)
        VALUES (p_id_locacao_criada, p_id_veiculo);

        UPDATE VEICULOS SET disponibilidade = 'Indisponível' WHERE id_veiculo = p_id_veiculo;

        SET p_mensagem_erro = NULL;
        COMMIT;
    END IF;

END$$

-- Restaura o delimitador padrão
DELIMITER ;


-- -----------------------------------------------------
-- Função Calcular preço final
-- -----------------------------------------------------
-- Define um novo delimitador
DELIMITER $$

CREATE FUNCTION fn_CalcularPrecoFinal(
    p_id_locacao INT
)
RETURNS DECIMAL(10, 2)
DETERMINISTIC
BEGIN
    -- Variáveis para guardar os dados que vamos buscar no banco
    DECLARE v_data_inicio DATETIME;
    DECLARE v_data_devolucao_efetiva DATETIME;
    DECLARE v_preco_diaria DECIMAL(10, 2);
    DECLARE v_dias_alugados INT;
    DECLARE v_preco_final DECIMAL(10, 2);

    -- Busca os dados necessários das tabelas para o cálculo
    SELECT 
        l.data_inicio, l.data_devolucao_efetiva, v.preco_diaria
    INTO 
        v_data_inicio, v_data_devolucao_efetiva, v_preco_diaria
    FROM 
        LOCACOES l
    JOIN 
        LOCACAO_VEICULOS lv ON l.id_locacao = lv.id_locacao_fk
    JOIN 
        VEICULOS v ON lv.id_veiculo_fk = v.id_veiculo
    WHERE 
        l.id_locacao = p_id_locacao;
    
    -- Se a data de devolução ainda não foi preenchida, não há o que calcular.
    IF v_data_devolucao_efetiva IS NULL THEN
        RETURN NULL;
    END IF;

    -- Calcula o número de dias. DATEDIFF retorna a diferença em dias.
    -- Somamos 1 para garantir que aluguéis de menos de 24h contem como 1 diária.
    SET v_dias_alugados = DATEDIFF(v_data_devolucao_efetiva, v_data_inicio) + 1;

    -- Garante que o mínimo de dias cobrados seja sempre 1.
    IF v_dias_alugados <= 0 THEN
        SET v_dias_alugados = 1;
    END IF;

    -- Aplica a lógica: quantidade de dias * preço diária
    SET v_preco_final = v_dias_alugados * v_preco_diaria;

    RETURN v_preco_final;
END$$

-- Restaura o delimitador padrão
DELIMITER ;




-- -----------------------------------------------------
-- TRIGGER
-- -----------------------------------------------------

DELIMITER $$
CREATE TRIGGER trg_AposInserirManutencao
AFTER INSERT ON MANUTENCOES
FOR EACH ROW
BEGIN
    -- Atualiza o status do veículo para 'Indisponível'
    UPDATE VEICULOS
    SET disponibilidade = 'Indisponível' 
    WHERE id_veiculo = NEW.id_veiculo_fk;
END$$
DELIMITER ;

