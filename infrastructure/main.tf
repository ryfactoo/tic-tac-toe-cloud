terraform {
  required_providers {
    aws = { # Określ wymaganą wersję dostawcy AWS
      source  = "hashicorp/aws"
      version = ">= 5.1"
    }
  }
  required_version = ">= 1.2.0" # Określ wymaganą wersję Terraforma
}

provider "aws" {
  region = "us-east-1" # Skonfiguruj dostawcę AWS dla regionu us-east-1.
}

module "tic_tac_toe_vpc" { # Stwórz moduł VPC
  source         = "terraform-aws-modules/vpc/aws"
  name           = "tic-tac-toe-vpc"
  cidr           = "10.0.0.0/16"
  azs            = ["us-east-1b"]
  public_subnets = ["10.0.101.0/24"]
  tags = {
    Terraform   = "true"
    Environment = "dev"
  }
}

resource "aws_security_group" "allow_ssh_http" { # Stwórz grupę zabezpieczeń dla SSH i HTTP
  name        = "allow_ssh_http"
  description = "Allow SSH and HTTP inbound traffic and all outbound traffic"
  vpc_id      = module.tic_tac_toe_vpc.vpc_id
  tags = {
    Name = "allow-ssh-http"
  }
}

resource "aws_vpc_security_group_egress_rule" "allow_all_traffic_ipv4" { # Dodaj regułę egress dla wszystkich ruchów wychodzących IPv4
  security_group_id = aws_security_group.allow_ssh_http.id
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "-1" # wszystkie porty
}
resource "aws_vpc_security_group_ingress_rule" "allow_http_backend" { # Dodaj regułę ingress dla ruchu TCP na porcie 8080
  security_group_id = aws_security_group.allow_ssh_http.id
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "tcp"
  from_port         = 8080
  to_port           = 8080
}

resource "aws_vpc_security_group_ingress_rule" "allow_http_frontend" { # Dodaj regułę ingress dla ruchu TCP na porcie 3000
  security_group_id = aws_security_group.allow_ssh_http.id
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "tcp"
  from_port         = 3000
  to_port           = 3000
}

resource "aws_vpc_security_group_ingress_rule" "allow_ssh" { # Dodaj regułę ingress dla ruchu SSH na porcie 22
  security_group_id = aws_security_group.allow_ssh_http.id
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "tcp"
  from_port         = 22
  to_port           = 22
}

resource "aws_key_pair" "app_key_pair" { # Utwórz parę kluczy AWS do połączenia SSH
  key_name   = "app_key_pair"
  public_key = file("keys/id_rsa.pub")
}

resource "aws_instance" "app_server" { # Utwórz instancję AWS
  ami                         = "ami-051f8a213df8bc089"
  instance_type               = "t2.micro"
  subnet_id                   = module.tic_tac_toe_vpc.public_subnets[0]
  associate_public_ip_address = "true"
  vpc_security_group_ids      = [aws_security_group.allow_ssh_http.id]
  key_name                    = aws_key_pair.app_key_pair.key_name

  connection { # Konfiguracja połączenia SSH
    type        = "ssh"
    user        = "ec2-user"
    private_key = file("keys/id_rsa")
    host        = self.public_ip
  }

  provisioner "remote-exec" { # Provisioner do zdalnego wykonania instalacji Docker i Docker Compose
    inline = [
      "sudo yum update -y",
      "sudo yum install docker -y",
      "sudo service docker start",
      "sudo usermod -aG docker ec2-user", # Zapewnia, że ​​ec2-user ma uprawnienia do wykonywania poleceń Docker
      "sudo curl -L https://github.com/docker/compose/releases/download/v2.26.1/docker-compose-linux-x86_64 -o /usr/local/bin/docker-compose",
      "sudo chmod +x /usr/local/bin/docker-compose",
    ]
  }

  provisioner "file" { # Provisioner do przesyłania pliku compose.yaml
    source      = "../compose.yaml"
    destination = "/home/ec2-user/compose.yaml"
  }

  provisioner "file" { # Provisioner do przesyłania pliku docker-compose.service
    source      = "../docker-compose.service"
    destination = "/tmp/docker-compose.service"
  }

  provisioner "remote-exec" { # Provisioner do konfiguracji usługi Docker Compose
    inline = [
      "sudo mv /tmp/docker-compose.service /etc/systemd/system/docker-compose.service", # Przenieś z podniesionymi uprawnieniami
      "sudo chown root:root /etc/systemd/system/docker-compose.service",                # Upewnij się, że właściciel jest poprawny
      "sudo systemctl daemon-reload",
      "sudo systemctl enable docker-compose.service",
      "sudo systemctl start docker-compose.service"
    ]
  }
}
