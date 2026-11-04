package chatapp.compose

import rego.v1

# SEC-09: cam container chay privileged
deny contains msg if {
 some name, service in input.services
 service.privileged == true
 msg := sprintf("%s: privileged containers are forbidden", [name])
}

# Bat buoc security_opt: no-new-privileges:true
deny contains msg if {
 some name, service in input.services
 not has_no_new_privileges(service)
 msg := sprintf("%s: no-new-privileges is required", [name])
}

# Bat buoc cap_drop: [ALL]
deny contains msg if {
 some name, service in input.services
 not drops_all_capabilities(service)
 msg := sprintf("%s: all Linux capabilities must be dropped", [name])
}

# Cong publish chi duoc bind 127.0.0.1 (thieu host_ip = mo moi interface -> vi pham)
deny contains msg if {
 some name, service in input.services
 some binding in service.ports
 not binding.host_ip == "127.0.0.1"
 msg := sprintf("%s: published ports must bind to 127.0.0.1", [name])
}

# SEC-10: db khong duoc publish cong ra host
deny contains msg if {
 some name, service in input.services
 name == "db"
 count(service.ports) > 0
 msg := "db: do not publish PostgreSQL ports to the host"
}

has_no_new_privileges(service) if "no-new-privileges:true" in service.security_opt

drops_all_capabilities(service) if "ALL" in service.cap_drop
