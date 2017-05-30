
function ViewModel()
{
    var self = this;

    self.Roles = ko.observableArray();
    self.EditingRole = ko.observable(new RoleModel({ Id: "", RoleName: "" }));
    
    self.setEditingRole = function (role) {
        role.ModalTitle("Edit Role: " + role.RoleName());
        self.EditingRole(role);
    }

    self.beginEditingRole = function () {       
        self.EditingRole().beginEdit();
    }

    self.saveRole = function (role) {
        if (!isValidateForm())
            return;
    }

    self.populateRoles = function (roles) {
        self.Roles.removeAll();
        $.each(roles, function (i, role) {
            self.Roles.push(new RoleModel(role));
        });
    }

    self.onClosingDialog = function () {
        self.EditingRole().rollback();
        self.EditingRole(undefined);
    }
}

function RoleModel(role) {
    var self = this;

    ko.mapping.fromJS(role, {}, self);

    self.ModalTitle = ko.observable();

    // enable ko.editables
    ko.editable(self);
}