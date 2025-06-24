$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        ajax: { url: '/admin/user/GetAlll'},
        "columns": [
            { data: 'userName',"width":"25%"},
            { data: 'email', "width": "15%" },
            { data: 'phoneNumber', "width": "10%"},
            { data: 'company.name', "width": "20%" },
            { data: 'role', "width": "20%" },
            {

                data: {id: "id",lockoutEnd:"lockoutEnd" },
                "render": function (data) {

                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();
                    if (lockout > today) {
                        return `
                       
                         <div class=text-center>
                             <a onclick=lockUnLock('${data.id}') class="btn btn-danger text-white" style="cursor:pointer;width:100px;">
                             <i class="bi bi-unlock-fill"></i> lock
                             </a>
                        </div>
                        <a href=/admin/user/RoleManagment?userId=${data.id} class="btn btn-danger text-white style="cursor:pointer;width:200px;">
                            <i class="bi bi-pencil-square"></i> Permission
                        </a>

                        `
                    }
                    else {
                        return `
                       

                         <div class=text-center>
                             <a onclick=lockUnLock('${data.id}') class="btn btn-success text-white" style="cursor:pointer;width:100px;">
                             <i class="bi bi-unlock-fill"></i> Unlock
                             </a>
                        </div>

                        <a  href=/admin/user/RoleManagment?userId=${data.id} class="btn btn-danger text-white style="cursor:pointer;width:200px;">
                            <i class="bi bi-pencil-square"></i> Permission
                        </a>

                        `
                    }

                },
                "width": "10%"
            }

            
        ]
    });
}

function lockUnLock(id) {
    $.ajax({
        type: "POST",
        url: '/admin/user/LockUnLock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            toastr.success(data.message);
            dataTable.ajax.reload();
        }
    });
}
                   